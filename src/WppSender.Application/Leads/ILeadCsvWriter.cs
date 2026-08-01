using WppSender.Domain;

namespace WppSender.Application.Leads;

public interface ILeadCsvWriter
{
    Task EscreverAsync(Stream destino, IAsyncEnumerable<Lead> leads);
}
