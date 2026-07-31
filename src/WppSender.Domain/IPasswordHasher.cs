namespace WppSender.Domain;

public interface IPasswordHasher
{
    string Hash(string senhaPlana);
    bool Verify(string senhaPlana, string hash);
}
