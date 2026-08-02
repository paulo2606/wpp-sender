namespace WppSender.Application.Grupos;

public class ListaGruposResultado
{
    public IReadOnlyList<GrupoResumo> Itens { get; }
    public int Total { get; }
    public int Pagina { get; }
    public int TamanhoPagina { get; }

    public ListaGruposResultado(IReadOnlyList<GrupoResumo> itens, int total, int pagina, int tamanhoPagina)
    {
        Itens = itens;
        Total = total;
        Pagina = pagina;
        TamanhoPagina = tamanhoPagina;
    }
}
