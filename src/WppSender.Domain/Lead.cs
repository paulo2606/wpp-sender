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
        Id = id;
        Nome = nome;
        TelefoneNormalizado = NormalizarTelefone(telefone);
        Instagram = instagram;
        Endereco = endereco;
        Origem = origem;
        DeletadoEm = null;
    }

    public void AtualizarDados(string nome, string telefone, string? instagram, Endereco? endereco, string? origem)
    {
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
}
