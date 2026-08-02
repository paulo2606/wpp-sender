using WppSender.Domain;

namespace WppSender.Application.Campanhas;

public class PausarCampanhaUseCase
{
    private const string MensagemNaoEncontrada = "Campanha não encontrada";
    private const string MensagemStatusInvalido = "Campanha só pode ser pausada quando está em andamento";

    private readonly ICampanhaRepository _repositorio;

    public PausarCampanhaUseCase(ICampanhaRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<PausarCampanhaResult> ExecutarAsync(Guid campanhaId)
    {
        var campanha = await _repositorio.BuscarPorIdAsync(campanhaId);
        if (campanha is null)
        {
            return PausarCampanhaResult.Falha(MensagemNaoEncontrada, PausarCampanhaErro.NaoEncontrada);
        }

        try
        {
            campanha.Pausar();
        }
        catch (InvalidOperationException)
        {
            return PausarCampanhaResult.Falha(MensagemStatusInvalido, PausarCampanhaErro.StatusInvalido);
        }

        await _repositorio.AtualizarAsync(campanha);

        return PausarCampanhaResult.ComSucesso();
    }
}
