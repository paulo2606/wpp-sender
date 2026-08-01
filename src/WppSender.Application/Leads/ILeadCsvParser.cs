namespace WppSender.Application.Leads;

public interface ILeadCsvParser
{
    IEnumerable<LeadCsvLinha> Parse(Stream csv);
}
