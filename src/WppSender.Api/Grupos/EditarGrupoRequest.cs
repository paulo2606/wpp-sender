namespace WppSender.Api.Grupos;

public record EditarGrupoRequest(string Nome, string? Descricao, IReadOnlyList<Guid>? LeadIds = null);
