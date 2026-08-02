namespace WppSender.Application.Grupos;

public class ExcluirGrupoResult
{
    public bool Sucesso { get; }
    public string? MensagemErro { get; }

    private ExcluirGrupoResult(bool sucesso, string? mensagemErro)
    {
        Sucesso = sucesso;
        MensagemErro = mensagemErro;
    }

    public static ExcluirGrupoResult ComSucesso() => new(true, null);

    public static ExcluirGrupoResult Falha(string mensagem) => new(false, mensagem);
}
