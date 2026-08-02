namespace WppSender.Application.Campanhas;

public class EditarCampanhaResult
{
    public bool Sucesso { get; }
    public string? MensagemErro { get; }
    public EditarCampanhaErro? Erro { get; }

    private EditarCampanhaResult(bool sucesso, string? mensagemErro, EditarCampanhaErro? erro)
    {
        Sucesso = sucesso;
        MensagemErro = mensagemErro;
        Erro = erro;
    }

    public static EditarCampanhaResult ComSucesso() => new(true, null, null);

    public static EditarCampanhaResult Falha(string mensagem, EditarCampanhaErro? erro = null) => new(false, mensagem, erro);
}
