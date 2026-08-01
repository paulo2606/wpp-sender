namespace WppSender.Api.Leads;

public record ImportarLeadsResponse(int Importados, IReadOnlyList<LeadPuladoResponse> Puladas);
