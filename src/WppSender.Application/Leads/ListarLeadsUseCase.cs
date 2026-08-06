using WppSender.Domain;

namespace WppSender.Application.Leads;

public class ListarLeadsUseCase
{
    private readonly ILeadRepository _repositorio;

    public ListarLeadsUseCase(ILeadRepository repositorio)
    {
        _repositorio = repositorio;
    }

    private const int TamanhoPaginaMinimo = 1;
    private const int TamanhoPaginaMaximo = 100;

    public async Task<ListaLeadsResultado> ExecutarAsync(string? busca, int pagina, int tamanhoPagina, Guid? grupoId = null)
    {

        var paginaValida = Math.Max(pagina, TamanhoPaginaMinimo);
        var tamanhoPaginaValido = Math.Clamp(tamanhoPagina, TamanhoPaginaMinimo, TamanhoPaginaMaximo);

        var (itens, total) = await _repositorio.ListarAsync(busca, paginaValida, tamanhoPaginaValido, grupoId);
        var resumos = itens
            .Select(LeadResumo.DeLead)
            .ToList();

        return new ListaLeadsResultado(resumos, total, paginaValida, tamanhoPaginaValido);
    }
}
