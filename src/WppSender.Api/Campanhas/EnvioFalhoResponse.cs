namespace WppSender.Api.Campanhas;

public record EnvioFalhoResponse(Guid EnvioId, Guid LeadId, string? NomeLead, string? Erro);
