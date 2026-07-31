using WppSender.Domain;

namespace WppSender.Application.Auth;

public interface IJwtTokenGenerator
{
    string GerarToken(Usuario usuario);
}
