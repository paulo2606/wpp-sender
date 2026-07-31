namespace WppSender.Application.Auth;

public class AutenticacaoResult
{
    public bool Sucesso { get; }
    public string? Token { get; }
    public string? MensagemErro { get; }

    private AutenticacaoResult(bool sucesso, string? token, string? mensagemErro)
    {
        Sucesso = sucesso;
        Token = token;
        MensagemErro = mensagemErro;
    }

    public static AutenticacaoResult ComSucesso(string token) => new(true, token, null);

    public static AutenticacaoResult Falha(string mensagem) => new(false, null, mensagem);
}
