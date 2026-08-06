using WppSender.Domain;
using WppSender.Application.Shared;

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

    public async Task<Resultado<PausarCampanhaErro>> ExecutarAsync(Guid campanhaId)
    {
        var campanha = await _repositorio.BuscarPorIdAsync(campanhaId);
        if (campanha is null)
        {
            return Resultado<PausarCampanhaErro>.Falha(MensagemNaoEncontrada, PausarCampanhaErro.NaoEncontrada);
        }

        try
        {
            campanha.Pausar();
        }
        catch (InvalidOperationException)
        {
            return Resultado<PausarCampanhaErro>.Falha(MensagemStatusInvalido, PausarCampanhaErro.StatusInvalido);
        }

        await _repositorio.AtualizarAsync(campanha);

        return Resultado<PausarCampanhaErro>.ComSucesso();
    }
}
