namespace WppSender.Api.Campanhas;

public record ListaCampanhasResponse(IReadOnlyList<CampanhaResponse> Itens, int Total, int Pagina, int TamanhoPagina);
