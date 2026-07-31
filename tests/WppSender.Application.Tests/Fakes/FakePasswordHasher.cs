using WppSender.Domain;

namespace WppSender.Application.Tests.Fakes;

public class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string senha) => $"hash-de-{senha}";

    public bool Verify(string senha, string hash) => hash == $"hash-de-{senha}";
}
