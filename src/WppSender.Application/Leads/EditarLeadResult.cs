namespace WppSender.Application.Leads;

public class EditarLeadResult
{
    public bool Sucesso { get; }
    public string? MensagemErro { get; }

    private EditarLeadResult(bool sucesso, string? mensagemErro)
    {
        Sucesso = sucesso;
        MensagemErro = mensagemErro;
    }

    public static EditarLeadResult ComSucesso() => new(true, null);

    public static EditarLeadResult Falha(string mensagem) => new(false, mensagem);
}
