using WppSender.Application.Leads;
using WppSender.Domain;

namespace WppSender.Application.Tests.Fakes;

public class FakeLeadCsvWriter : ILeadCsvWriter
{
    public List<LeadExportavel> LeadsExportados { get; } = new();

    public IReadOnlyList<Lead> LeadsRecebidos => LeadsExportados.Select(e => e.Lead).ToList();

    public async Task EscreverAsync(Stream destino, IAsyncEnumerable<LeadExportavel> leads)
    {
        await foreach (var exportavel in leads)
        {
            LeadsExportados.Add(exportavel);
        }
    }
}
