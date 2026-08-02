using WppSender.Domain;

namespace WppSender.Application.Campanhas;

public class ListarCampanhasUseCase
{
    private const int PaginaMinima = 1;
    private const int TamanhoPaginaMinimo = 1;
    private const int TamanhoPaginaMaximo = 100;

    private readonly ICampanhaRepository _repositorio;

    public ListarCampanhasUseCase(ICampanhaRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<ListaCampanhasResultado> ExecutarAsync(StatusCampanha? status, int pagina, int tamanhoPagina)
    {
        var paginaValida = Math.Max(pagina, PaginaMinima);
        var tamanhoPaginaValido = Math.Clamp(tamanhoPagina, TamanhoPaginaMinimo, TamanhoPaginaMaximo);

        var (itens, total) = await _repositorio.ListarAsync(status, paginaValida, tamanhoPaginaValido);
        var resumos = itens.Select(CampanhaResumo.DeCampanha).ToList();

        return new ListaCampanhasResultado(resumos, total, paginaValida, tamanhoPaginaValido);
    }
}
