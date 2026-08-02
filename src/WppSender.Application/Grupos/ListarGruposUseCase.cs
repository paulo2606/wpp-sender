using WppSender.Domain;

namespace WppSender.Application.Grupos;

public class ListarGruposUseCase
{
    private const int PaginaMinima = 1;
    private const int TamanhoPaginaMinimo = 1;
    private const int TamanhoPaginaMaximo = 100;

    private readonly IGrupoRepository _repositorio;

    public ListarGruposUseCase(IGrupoRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<ListaGruposResultado> ExecutarAsync(int pagina, int tamanhoPagina)
    {
        var paginaValida = Math.Max(pagina, PaginaMinima);
        var tamanhoPaginaValido = Math.Clamp(tamanhoPagina, TamanhoPaginaMinimo, TamanhoPaginaMaximo);

        var (itens, total) = await _repositorio.ListarComContagemAsync(paginaValida, tamanhoPaginaValido);
        var resumos = itens
            .Select(i => new GrupoResumo(i.Grupo.Id, i.Grupo.Nome, i.Grupo.Descricao, i.QuantidadeLeads))
            .ToList();

        return new ListaGruposResultado(resumos, total, paginaValida, tamanhoPaginaValido);
    }
}
