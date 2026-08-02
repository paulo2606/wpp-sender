using WppSender.Domain;

namespace WppSender.Application.Leads;

public record LeadExportavel(Lead Lead, string? NomeGrupo);
