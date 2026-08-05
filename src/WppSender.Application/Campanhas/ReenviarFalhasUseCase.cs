using WppSender.Domain;

namespace WppSender.Application.Campanhas;

public class ReenviarFalhasUseCase
{
    private readonly ICampanhaRepository _campanhaRepositorio;
    private readonly IEnvioRepository _envioRepositorio;
    private readonly ICampanhaJobScheduler _jobScheduler;

    public ReenviarFalhasUseCase(ICampanhaRepository campanhaRepositorio, IEnvioRepository envioRepositorio, ICampanhaJobScheduler jobScheduler)
    {
        _campanhaRepositorio = campanhaRepositorio;
        _envioRepositorio = envioRepositorio;
        _jobScheduler = jobScheduler;
    }

    public async Task ExecutarAsync(Guid campanhaId)
    {
        var falhos = await _envioRepositorio.ListarFalhosAsync(campanhaId);
        if (falhos.Count == 0)
        {
            return;
        }

        foreach (var envio in falhos)
        {
            envio.Resetar();
            await _envioRepositorio.AtualizarAsync(envio);
        }

        var campanha = await _campanhaRepositorio.BuscarPorIdAsync(campanhaId);
        if (campanha is null)
        {
            return;
        }

        campanha.ReabrirParaReenvio();
        await _campanhaRepositorio.AtualizarAsync(campanha);

        // Sempre agenda um passo: mesmo que a campanha já estivesse EmAndamento, a cadeia de
        // disparo pode ter ficado dormente (rodou todos os pendentes e parou de reagendar).
        // Isso é seguro mesmo se a cadeia ainda estiver ativa — o lock distribuído por campanha
        // em CampanhaSendJob serializa as execuções, então uma chamada "extra" que não encontra
        // mais nada pendente simplesmente não faz nada, sem duplicar envios nem acelerar o ritmo.
        await _jobScheduler.AgendarProximoEnvioAsync(campanhaId, TimeSpan.Zero);
    }
}
