using WppSender.Domain;

namespace WppSender.Application.Campanhas;

public class CancelarCampanhaUseCase
{
    private const string MensagemNaoEncontrada = "Campanha não encontrada";
    private const string MensagemStatusInvalido = "Campanha só pode ser cancelada quando está pausada";
    private const string MotivoFalha = "Campanha cancelada";

    private readonly ICampanhaRepository _campanhaRepositorio;
    private readonly IEnvioRepository _envioRepositorio;

    public CancelarCampanhaUseCase(ICampanhaRepository campanhaRepositorio, IEnvioRepository envioRepositorio)
    {
        _campanhaRepositorio = campanhaRepositorio;
        _envioRepositorio = envioRepositorio;
    }

    public async Task<CancelarCampanhaResult> ExecutarAsync(Guid campanhaId)
    {
        var campanha = await _campanhaRepositorio.BuscarPorIdAsync(campanhaId);
        if (campanha is null)
        {
            return CancelarCampanhaResult.Falha(MensagemNaoEncontrada, CancelarCampanhaErro.NaoEncontrada);
        }

        try
        {
            campanha.Cancelar();
        }
        catch (InvalidOperationException)
        {
            return CancelarCampanhaResult.Falha(MensagemStatusInvalido, CancelarCampanhaErro.StatusInvalido);
        }

        // Envios ainda pendentes não serão mais disparados: viram falha.
        // Envios já "Enviado" (despachados, aguardando confirmação) não são tocados aqui —
        // o AtualizarStatusEntregaUseCase para de rastreá-los assim que a campanha vira Cancelada.
        var pendente = await _envioRepositorio.BuscarProximoPendenteAsync(campanhaId);
        while (pendente is not null)
        {
            pendente.MarcarComoFalhou(MotivoFalha);
            await _envioRepositorio.AtualizarAsync(pendente);
            pendente = await _envioRepositorio.BuscarProximoPendenteAsync(campanhaId);
        }

        await _campanhaRepositorio.AtualizarAsync(campanha);

        return CancelarCampanhaResult.ComSucesso();
    }
}
