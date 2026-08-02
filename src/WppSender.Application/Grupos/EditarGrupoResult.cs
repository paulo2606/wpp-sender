namespace WppSender.Application.Grupos;

public class EditarGrupoResult
{
    public bool Sucesso { get; }
    public string? MensagemErro { get; }

    private EditarGrupoResult(bool sucesso, string? mensagemErro)
    {
        Sucesso = sucesso;
        MensagemErro = mensagemErro;
    }

    public static EditarGrupoResult ComSucesso() => new(true, null);

    public static EditarGrupoResult Falha(string mensagem) => new(false, mensagem);
}
