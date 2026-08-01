namespace WppSender.Application.Leads;

public class ListaLeadsResultado
{
    public IReadOnlyList<LeadResumo> Itens { get; }
    public int Total { get; }
    public int Pagina { get; }
    public int TamanhoPagina { get; }

    public ListaLeadsResultado(IReadOnlyList<LeadResumo> itens, int total, int pagina, int tamanhoPagina)
    {
        Itens = itens;
        Total = total;
        Pagina = pagina;
        TamanhoPagina = tamanhoPagina;
    }
}
