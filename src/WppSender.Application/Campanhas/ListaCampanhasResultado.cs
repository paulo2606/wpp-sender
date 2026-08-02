namespace WppSender.Application.Campanhas;

public class ListaCampanhasResultado
{
    public IReadOnlyList<CampanhaResumo> Itens { get; }
    public int Total { get; }
    public int Pagina { get; }
    public int TamanhoPagina { get; }

    public ListaCampanhasResultado(IReadOnlyList<CampanhaResumo> itens, int total, int pagina, int tamanhoPagina)
    {
        Itens = itens;
        Total = total;
        Pagina = pagina;
        TamanhoPagina = tamanhoPagina;
    }
}
