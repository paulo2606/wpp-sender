using WppSender.Domain;
using WppSender.Application.Shared;

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

    public async Task<Resultado<ExcluirCampanhaErro>> ExecutarAsync(Guid id)
    {
        var campanha = await _repositorio.BuscarPorIdAsync(id);
        if (campanha is null)
        {
            return Resultado<ExcluirCampanhaErro>.Falha(MensagemNaoEncontrada, ExcluirCampanhaErro.NaoEncontrada);
        }

        if (!campanha.PodeExcluir())
        {
            return Resultado<ExcluirCampanhaErro>.Falha(MensagemNaoPermiteExclusao, ExcluirCampanhaErro.NaoPermiteExclusao);
        }

        await _repositorio.RemoverAsync(campanha);

        return Resultado<ExcluirCampanhaErro>.ComSucesso();
    }
}
