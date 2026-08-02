namespace WppSender.Api.Grupos;

public record CriarGrupoRequest(string Nome, string? Descricao, IReadOnlyList<Guid> LeadIds);
