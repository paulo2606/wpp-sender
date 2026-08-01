namespace WppSender.Application.Leads;

public class ObterLeadResult
{
    public bool Sucesso { get; }
    public LeadResumo? Lead { get; }
    public string? MensagemErro { get; }

    private ObterLeadResult(bool sucesso, LeadResumo? lead, string? mensagemErro)
    {
        Sucesso = sucesso;
        Lead = lead;
        MensagemErro = mensagemErro;
    }

    public static ObterLeadResult ComSucesso(LeadResumo lead) => new(true, lead, null);

    public static ObterLeadResult Falha(string mensagem) => new(false, null, mensagem);
}
