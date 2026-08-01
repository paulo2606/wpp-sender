namespace WppSender.Domain;

public class Endereco
{
    public Guid Id { get; private set; }
    public string Rua { get; private set; }
    public string Numero { get; private set; }
    public string? Complemento { get; private set; }
    public string Bairro { get; private set; }
    public string Cidade { get; private set; }
    public string Estado { get; private set; }
    public string Cep { get; private set; }

    public Endereco(Guid id, string rua, string numero, string? complemento, string bairro, string cidade, string estado, string cep)
    {
        Id = id;
        Rua = rua;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cidade = cidade;
        Estado = estado;
        Cep = cep;
    }

    // Muta a instância existente em vez de criar um novo Endereco, para que
    // EditarLeadUseCase possa reaproveitar a mesma linha na tabela enderecos
    // ao invés de órfão a antiga e criar uma nova a cada edição.
    public void AtualizarDados(string rua, string numero, string? complemento, string bairro, string cidade, string estado, string cep)
    {
        Rua = rua;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cidade = cidade;
        Estado = estado;
        Cep = cep;
    }
}
