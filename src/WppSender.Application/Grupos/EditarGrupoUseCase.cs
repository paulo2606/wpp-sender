using WppSender.Domain;

namespace WppSender.Application.Grupos;

public class EditarGrupoUseCase
{
    private const string MensagemGrupoNaoEncontrado = "Grupo não encontrado";

    private readonly IGrupoRepository _repositorio;

    public EditarGrupoUseCase(IGrupoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<EditarGrupoResult> ExecutarAsync(Guid id, string nome, string? descricao)
    {
        var grupo = await _repositorio.BuscarPorIdAsync(id);
        if (grupo is null)
        {
            return EditarGrupoResult.Falha(MensagemGrupoNaoEncontrado, EditarGrupoErro.NaoEncontrado);
        }

        try
        {
            grupo.AtualizarDados(nome, descricao);
        }
        catch (ArgumentException ex)
        {
            return EditarGrupoResult.Falha(ex.Message);
        }

        await _repositorio.AtualizarAsync(grupo);

        return EditarGrupoResult.ComSucesso();
    }
}
