namespace WppSender.Application.Leads;

public class ImportarLeadsResultado
{
    public int Importados { get; }
    public IReadOnlyList<LeadPulado> Puladas { get; }

    public ImportarLeadsResultado(int importados, IReadOnlyList<LeadPulado> puladas)
    {
        Importados = importados;
        Puladas = puladas;
    }
}
