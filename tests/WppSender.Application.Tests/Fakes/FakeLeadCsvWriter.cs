using WppSender.Application.Leads;
using WppSender.Domain;

namespace WppSender.Application.Tests.Fakes;

public class FakeLeadCsvWriter : ILeadCsvWriter
{
    public List<Lead> LeadsRecebidos { get; } = new();

    public async Task EscreverAsync(Stream destino, IAsyncEnumerable<Lead> leads)
    {
        await foreach (var lead in leads)
        {
            LeadsRecebidos.Add(lead);
        }
    }
}
