using WppSender.Domain;

namespace WppSender.Application.Leads;

public class ListarLeadsUseCase
{
    private readonly ILeadRepository _repositorio;

    public ListarLeadsUseCase(ILeadRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<ListaLeadsResultado> ExecutarAsync(string? busca, int pagina, int tamanhoPagina)
    {
        var (itens, total) = await _repositorio.ListarAsync(busca, pagina, tamanhoPagina);
        var resumos = itens
            .Select(l => new LeadResumo(l.Id, l.Nome, l.TelefoneNormalizado, l.Instagram, l.Origem))
            .ToList();

        return new ListaLeadsResultado(resumos, total);
    }
}
