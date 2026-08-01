using WppSender.Application.Leads;

namespace WppSender.Application.Tests.Fakes;

public class FakeLeadCsvParser : ILeadCsvParser
{
    private readonly IEnumerable<LeadCsvLinha> _linhas;

    public FakeLeadCsvParser(IEnumerable<LeadCsvLinha> linhas)
    {
        _linhas = linhas;
    }

    public IEnumerable<LeadCsvLinha> Parse(Stream csv) => _linhas;
}
