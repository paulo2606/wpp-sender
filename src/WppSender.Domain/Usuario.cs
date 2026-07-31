namespace WppSender.Domain;

public class Usuario
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string SenhaHash { get; private set; }

    public Usuario(Guid id, string email, string senhaHash)
    {
        Id = id;
        Email = email;
        SenhaHash = senhaHash;
    }

    public bool Autenticar(string senha, IPasswordHasher hasher)
    {
        return hasher.Verify(senha, SenhaHash);
    }
}
