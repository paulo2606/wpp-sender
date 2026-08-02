namespace WppSender.Application.Leads;

public interface ILeadCsvWriter
{
    Task EscreverAsync(Stream destino, IAsyncEnumerable<LeadExportavel> leads);
}
