using WppSender.Domain;
using WppSender.Application.Shared;

namespace WppSender.Application.Grupos;

public class ExcluirGrupoUseCase
{
    private const string MensagemGrupoNaoEncontrado = "Grupo não encontrado";

    private readonly IGrupoRepository _repositorio;

    public ExcluirGrupoUseCase(IGrupoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<Resultado> ExecutarAsync(Guid id)
    {
        var grupo = await _repositorio.BuscarPorIdAsync(id);
        if (grupo is null)
        {
            return Resultado.Falha(MensagemGrupoNaoEncontrado);
        }

        await _repositorio.RemoverAsync(grupo);

        return Resultado.ComSucesso();
    }
}
