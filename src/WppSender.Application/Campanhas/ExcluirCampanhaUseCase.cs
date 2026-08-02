using WppSender.Domain;

namespace WppSender.Application.Campanhas;

public class ExcluirCampanhaUseCase
{
    private const string MensagemNaoEncontrada = "Campanha não encontrada";
    private const string MensagemNaoPermiteExclusao = "Campanha só pode ser excluída em rascunho ou agendada";

    private readonly ICampanhaRepository _repositorio;

    public ExcluirCampanhaUseCase(ICampanhaRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<ExcluirCampanhaResult> ExecutarAsync(Guid id)
    {
        var campanha = await _repositorio.BuscarPorIdAsync(id);
        if (campanha is null)
        {
            return ExcluirCampanhaResult.Falha(MensagemNaoEncontrada, ExcluirCampanhaErro.NaoEncontrada);
        }

        if (!campanha.PodeExcluir())
        {
            return ExcluirCampanhaResult.Falha(MensagemNaoPermiteExclusao, ExcluirCampanhaErro.NaoPermiteExclusao);
        }

        await _repositorio.RemoverAsync(campanha);

        return ExcluirCampanhaResult.ComSucesso();
    }
}
