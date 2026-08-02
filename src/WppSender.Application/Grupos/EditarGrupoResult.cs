namespace WppSender.Application.Grupos;

public class EditarGrupoResult
{
    public bool Sucesso { get; }
    public string? MensagemErro { get; }

    // Tipo do erro, para o controller decidir o status HTTP sem comparar strings.
    // Fica nulo para falhas que não se encaixam em nenhuma das categorias conhecidas
    // (ex.: erro de validação de campos obrigatórios).
    public EditarGrupoErro? Erro { get; }

    private EditarGrupoResult(bool sucesso, string? mensagemErro, EditarGrupoErro? erro)
    {
        Sucesso = sucesso;
        MensagemErro = mensagemErro;
        Erro = erro;
    }

    public static EditarGrupoResult ComSucesso() => new(true, null, null);

    public static EditarGrupoResult Falha(string mensagem, EditarGrupoErro? erro = null) => new(false, mensagem, erro);
}
