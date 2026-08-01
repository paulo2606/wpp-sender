namespace WppSender.Application.Leads;

public class EditarLeadResult
{
    public bool Sucesso { get; }
    public string? MensagemErro { get; }

    // Tipo do erro, para o controller decidir o status HTTP sem comparar strings.
    // Fica nulo para falhas que não se encaixam em nenhuma das categorias conhecidas
    // (ex.: erro de validação de campos obrigatórios).
    public EditarLeadErro? Erro { get; }

    private EditarLeadResult(bool sucesso, string? mensagemErro, EditarLeadErro? erro)
    {
        Sucesso = sucesso;
        MensagemErro = mensagemErro;
        Erro = erro;
    }

    public static EditarLeadResult ComSucesso() => new(true, null, null);

    public static EditarLeadResult Falha(string mensagem, EditarLeadErro? erro = null) => new(false, mensagem, erro);
}
