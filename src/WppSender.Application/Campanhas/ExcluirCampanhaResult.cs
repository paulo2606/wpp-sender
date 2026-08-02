namespace WppSender.Application.Campanhas;

public class ExcluirCampanhaResult
{
    public bool Sucesso { get; }
    public string? MensagemErro { get; }
    public ExcluirCampanhaErro? Erro { get; }

    private ExcluirCampanhaResult(bool sucesso, string? mensagemErro, ExcluirCampanhaErro? erro)
    {
        Sucesso = sucesso;
        MensagemErro = mensagemErro;
        Erro = erro;
    }

    public static ExcluirCampanhaResult ComSucesso() => new(true, null, null);

    public static ExcluirCampanhaResult Falha(string mensagem, ExcluirCampanhaErro? erro = null) => new(false, mensagem, erro);
}
