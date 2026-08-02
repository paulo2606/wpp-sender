namespace WppSender.Application.Campanhas;

public class PausarCampanhaResult
{
    public bool Sucesso { get; }
    public string? MensagemErro { get; }
    public PausarCampanhaErro? Erro { get; }

    private PausarCampanhaResult(bool sucesso, string? mensagemErro, PausarCampanhaErro? erro)
    {
        Sucesso = sucesso;
        MensagemErro = mensagemErro;
        Erro = erro;
    }

    public static PausarCampanhaResult ComSucesso() => new(true, null, null);

    public static PausarCampanhaResult Falha(string mensagem, PausarCampanhaErro erro) => new(false, mensagem, erro);
}
