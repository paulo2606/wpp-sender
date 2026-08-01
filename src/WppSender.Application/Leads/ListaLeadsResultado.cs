namespace WppSender.Application.Leads;

public class ListaLeadsResultado
{
    public IReadOnlyList<LeadResumo> Itens { get; }
    public int Total { get; }

    public ListaLeadsResultado(IReadOnlyList<LeadResumo> itens, int total)
    {
        Itens = itens;
        Total = total;
    }
}
