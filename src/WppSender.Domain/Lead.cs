using System.Linq;

namespace WppSender.Domain;

public class Lead
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string TelefoneNormalizado { get; private set; }
    public string? Instagram { get; private set; }
    public Endereco? Endereco { get; private set; }
    public string? Origem { get; private set; }
    public DateTime? DeletadoEm { get; private set; }

    public bool EstaAtivo => DeletadoEm is null;

    public Lead(Guid id, string nome, string telefone, string? instagram, Endereco? endereco, string? origem)
    {
        ValidarCamposObrigatorios(nome, telefone);

        Id = id;
        Nome = nome;
        TelefoneNormalizado = NormalizarTelefone(telefone);
        Instagram = instagram;
        Endereco = endereco;
        Origem = origem;
        DeletadoEm = null;
    }

    // Construtor privado usado pelo EF Core para materializar a entidade.
    // Necessário porque o construtor público recebe a navegação Endereco,
    // que o EF Core não consegue vincular via construtor.
    private Lead()
    {
        Nome = null!;
        TelefoneNormalizado = null!;
    }

    public void AtualizarDados(string nome, string telefone, string? instagram, Endereco? endereco, string? origem)
    {
        ValidarCamposObrigatorios(nome, telefone);

        Nome = nome;
        TelefoneNormalizado = NormalizarTelefone(telefone);
        Instagram = instagram;
        Endereco = endereco;
        Origem = origem;
    }

    public void Excluir()
    {
        DeletadoEm = DateTime.UtcNow;
    }

    public static string NormalizarTelefone(string telefone)
    {
        return new string(telefone.Where(char.IsDigit).ToArray());
    }

    // Exposto como público para que casos de uso (ex.: EditarLeadUseCase) possam validar
    // nome/telefone antes de operações que dependem deles (como normalizar o telefone
    // para busca de duplicidade) sem duplicar a mensagem de erro em dois lugares.
    public static void ValidarCamposObrigatorios(string nome, string telefone)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome é obrigatório", nameof(nome));
        }

        if (string.IsNullOrWhiteSpace(telefone))
        {
            throw new ArgumentException("Telefone é obrigatório", nameof(telefone));
        }
    }
}
