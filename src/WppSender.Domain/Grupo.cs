namespace WppSender.Domain;

public class Grupo
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string? Descricao { get; private set; }

    public Grupo(Guid id, string nome, string? descricao)
    {
        ValidarNome(nome);

        Id = id;
        Nome = nome;
        Descricao = descricao;
    }

    public void AtualizarDados(string nome, string? descricao)
    {
        ValidarNome(nome);

        Nome = nome;
        Descricao = descricao;
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome é obrigatório", nameof(nome));
        }
    }
}
