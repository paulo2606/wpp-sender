using WppSender.Domain;

namespace WppSender.Application.Grupos;

public class ExcluirGrupoUseCase
{
    private const string MensagemGrupoNaoEncontrado = "Grupo não encontrado";

    private readonly IGrupoRepository _repositorio;

    public ExcluirGrupoUseCase(IGrupoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<ExcluirGrupoResult> ExecutarAsync(Guid id)
    {
        var grupo = await _repositorio.BuscarPorIdAsync(id);
        if (grupo is null)
        {
            return ExcluirGrupoResult.Falha(MensagemGrupoNaoEncontrado);
        }

        await _repositorio.RemoverAsync(grupo);

        return ExcluirGrupoResult.ComSucesso();
    }
}
