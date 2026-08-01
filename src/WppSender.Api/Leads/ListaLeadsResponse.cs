namespace WppSender.Api.Leads;

public record ListaLeadsResponse(IReadOnlyList<LeadResponse> Itens, int Total, int Pagina, int TamanhoPagina);
