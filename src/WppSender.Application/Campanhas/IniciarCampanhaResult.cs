namespace WppSender.Application.Campanhas;

public class IniciarCampanhaResult
{
    public bool Sucesso { get; }
    public string? MensagemErro { get; }
    public IniciarCampanhaErro? Erro { get; }

    private IniciarCampanhaResult(bool sucesso, string? mensagemErro, IniciarCampanhaErro? erro)
    {
        Sucesso = sucesso;
        MensagemErro = mensagemErro;
        Erro = erro;
    }

    public static IniciarCampanhaResult ComSucesso() => new(true, null, null);

    public static IniciarCampanhaResult Falha(string mensagem, IniciarCampanhaErro erro) => new(false, mensagem, erro);
}
