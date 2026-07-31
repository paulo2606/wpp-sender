using WppSender.Domain;

namespace WppSender.Application.Tests.Fakes;

public class FakePasswordHasher : IPasswordHasher
{
    public int VerifyCallCount { get; private set; }

    public string Hash(string senha) => $"hash-de-{senha}";

    public bool Verify(string senha, string hash)
    {
        VerifyCallCount++;
        return hash == $"hash-de-{senha}";
    }
}
