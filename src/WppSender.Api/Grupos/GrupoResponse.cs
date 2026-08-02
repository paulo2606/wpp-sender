namespace WppSender.Api.Grupos;

public record GrupoResponse(Guid Id, string Nome, string? Descricao, int QuantidadeLeads);
