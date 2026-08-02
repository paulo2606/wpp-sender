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

        // Se a campanha já está em andamento, o próprio encadeamento dela vai naturalmente
        // pegar os envios recém-resetados no próximo passo — agendar aqui criaria uma
        // segunda cadeia paralela e reduziria pela metade o intervalo anti-ban real.
        var jaEstavaEmAndamento = campanha.Status == StatusCampanha.EmAndamento;

        campanha.ReabrirParaReenvio();
        await _campanhaRepositorio.AtualizarAsync(campanha);

        if (!jaEstavaEmAndamento)
        {
            await _jobScheduler.AgendarProximoEnvioAsync(campanhaId, TimeSpan.Zero);
        }
    }
}
