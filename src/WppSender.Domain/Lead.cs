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
    public Guid? GrupoId { get; private set; }

    public bool EstaAtivo => DeletadoEm is null;

    public Lead(Guid id, string nome, string telefone, string? instagram, Endereco? endereco, string? origem, Guid? grupoId = null)
    {
        ValidarCamposObrigatorios(nome, telefone);

        Id = id;
        Nome = nome;
        TelefoneNormalizado = NormalizarTelefone(telefone);
        Instagram = instagram;
        Endereco = endereco;
        Origem = origem;
        DeletadoEm = null;
        GrupoId = grupoId;
    }

    // Construtor privado usado pelo EF Core para materializar a entidade.
    // Necessário porque o construtor público recebe a navegação Endereco,
    // que o EF Core não consegue vincular via construtor.
    private Lead()
    {
        Nome = null!;
        TelefoneNormalizado = null!;
    }

    public void AtualizarDados(string nome, string telefone, string? instagram, Endereco? endereco, string? origem, Guid? grupoId = null)
    {
        ValidarCamposObrigatorios(nome, telefone);

        Nome = nome;
        TelefoneNormalizado = NormalizarTelefone(telefone);
        Instagram = instagram;
        Endereco = endereco;
        Origem = origem;
        GrupoId = grupoId;
    }

    public void AtribuirGrupo(Guid? grupoId)
    {
        GrupoId = grupoId;
    }

    public void Excluir()
    {
        DeletadoEm = DateTime.UtcNow;
    }

    public static string NormalizarTelefone(string telefone)
    {
        return new string(telefone.Where(char.IsDigit).ToArray());
    }

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
