namespace WppSender.Application.Campanhas;

public class CriarCampanhaResult
{
    public bool Sucesso { get; }
    public Guid? CampanhaId { get; }
    public string? MensagemErro { get; }

    private CriarCampanhaResult(bool sucesso, Guid? campanhaId, string? mensagemErro)
    {
        Sucesso = sucesso;
        CampanhaId = campanhaId;
        MensagemErro = mensagemErro;
    }

    public static CriarCampanhaResult ComSucesso(Guid campanhaId) => new(true, campanhaId, null);

    public static CriarCampanhaResult Falha(string mensagem) => new(false, null, mensagem);
}
