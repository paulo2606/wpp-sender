namespace WppSender.Application.Grupos;

public class CriarGrupoResult
{
    public bool Sucesso { get; }
    public Guid? GrupoId { get; }
    public string? MensagemErro { get; }

    private CriarGrupoResult(bool sucesso, Guid? grupoId, string? mensagemErro)
    {
        Sucesso = sucesso;
        GrupoId = grupoId;
        MensagemErro = mensagemErro;
    }

    public static CriarGrupoResult ComSucesso(Guid grupoId) => new(true, grupoId, null);

    public static CriarGrupoResult Falha(string mensagem) => new(false, null, mensagem);
}
