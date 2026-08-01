namespace WppSender.Application.Leads;

public class CriarLeadResult
{
    public bool Sucesso { get; }
    public Guid? LeadId { get; }
    public string? MensagemErro { get; }

    private CriarLeadResult(bool sucesso, Guid? leadId, string? mensagemErro)
    {
        Sucesso = sucesso;
        LeadId = leadId;
        MensagemErro = mensagemErro;
    }

    public static CriarLeadResult ComSucesso(Guid leadId) => new(true, leadId, null);

    public static CriarLeadResult Falha(string mensagem) => new(false, null, mensagem);
}
