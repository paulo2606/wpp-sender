namespace WppSender.Application.Auth;

public class RegistroResult
{
    public bool Sucesso { get; }
    public string? MensagemErro { get; }

    private RegistroResult(bool sucesso, string? mensagemErro)
    {
        Sucesso = sucesso;
        MensagemErro = mensagemErro;
    }

    public static RegistroResult ComSucesso() => new(true, null);

    public static RegistroResult Falha(string mensagem) => new(false, mensagem);
}
