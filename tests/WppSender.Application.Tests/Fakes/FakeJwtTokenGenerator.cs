using WppSender.Application.Auth;
using WppSender.Domain;

namespace WppSender.Application.Tests.Fakes;

public class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public string GerarToken(Usuario usuario) => $"token-para-{usuario.Email}";
}
