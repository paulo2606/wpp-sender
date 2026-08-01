namespace WppSender.Api.Leads;

public record LeadResponse(Guid Id, string Nome, string Telefone, string? Instagram, string? Origem);
