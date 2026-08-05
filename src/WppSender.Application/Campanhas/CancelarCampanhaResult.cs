namespace WppSender.Application.Campanhas;

public class CancelarCampanhaResult
{
    public bool Sucesso { get; }
    public string? MensagemErro { get; }
    public CancelarCampanhaErro? Erro { get; }

    private CancelarCampanhaResult(bool sucesso, string? mensagemErro, CancelarCampanhaErro? erro)
    {
        Sucesso = sucesso;
        MensagemErro = mensagemErro;
        Erro = erro;
    }

    public static CancelarCampanhaResult ComSucesso() => new(true, null, null);

    public static CancelarCampanhaResult Falha(string mensagem, CancelarCampanhaErro erro) => new(false, mensagem, erro);
}
