namespace WppSender.Application.Campanhas;

public class RetomarCampanhaResult
{
    public bool Sucesso { get; }
    public string? MensagemErro { get; }
    public RetomarCampanhaErro? Erro { get; }

    private RetomarCampanhaResult(bool sucesso, string? mensagemErro, RetomarCampanhaErro? erro)
    {
        Sucesso = sucesso;
        MensagemErro = mensagemErro;
        Erro = erro;
    }

    public static RetomarCampanhaResult ComSucesso() => new(true, null, null);

    public static RetomarCampanhaResult Falha(string mensagem, RetomarCampanhaErro erro) => new(false, mensagem, erro);
}
