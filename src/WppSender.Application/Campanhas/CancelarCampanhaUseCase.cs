using WppSender.Domain;
using WppSender.Application.Shared;

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

    public async Task<Resultado<CancelarCampanhaErro>> ExecutarAsync(Guid campanhaId)
    {
        var campanha = await _campanhaRepositorio.BuscarPorIdAsync(campanhaId);
        if (campanha is null)
        {
            return Resultado<CancelarCampanhaErro>.Falha(MensagemNaoEncontrada, CancelarCampanhaErro.NaoEncontrada);
        }

        try
        {
            campanha.Cancelar();
        }
        catch (InvalidOperationException)
        {
            return Resultado<CancelarCampanhaErro>.Falha(MensagemStatusInvalido, CancelarCampanhaErro.StatusInvalido);
        }

        var pendente = await _envioRepositorio.BuscarProximoPendenteAsync(campanhaId);
        while (pendente is not null)
        {
            pendente.MarcarComoFalhou(MotivoFalha);
            await _envioRepositorio.AtualizarAsync(pendente);
            pendente = await _envioRepositorio.BuscarProximoPendenteAsync(campanhaId);
        }

        await _campanhaRepositorio.AtualizarAsync(campanha);

        return Resultado<CancelarCampanhaErro>.ComSucesso();
    }
}
