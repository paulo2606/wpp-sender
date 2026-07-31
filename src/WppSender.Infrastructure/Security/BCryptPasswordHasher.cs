using WppSender.Domain;

namespace WppSender.Infrastructure.Security;

public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string senhaPlana) => BCrypt.Net.BCrypt.HashPassword(senhaPlana);

    public bool Verify(string senhaPlana, string hash) => BCrypt.Net.BCrypt.Verify(senhaPlana, hash);
}
