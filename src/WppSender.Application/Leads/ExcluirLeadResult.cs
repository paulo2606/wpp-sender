namespace WppSender.Application.Leads;

public class ExcluirLeadResult
{
    public bool Sucesso { get; }
    public string? MensagemErro { get; }

    private ExcluirLeadResult(bool sucesso, string? mensagemErro)
    {
        Sucesso = sucesso;
        MensagemErro = mensagemErro;
    }

    public static ExcluirLeadResult ComSucesso() => new(true, null);

    public static ExcluirLeadResult Falha(string mensagem) => new(false, mensagem);
}
