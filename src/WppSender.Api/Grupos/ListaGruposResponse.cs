namespace WppSender.Api.Grupos;

public record ListaGruposResponse(IReadOnlyList<GrupoResponse> Itens, int Total, int Pagina, int TamanhoPagina);
