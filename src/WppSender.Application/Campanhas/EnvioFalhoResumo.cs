namespace WppSender.Application.Campanhas;

public record EnvioFalhoResumo(Guid EnvioId, Guid LeadId, string? NomeLead, string? Erro);
