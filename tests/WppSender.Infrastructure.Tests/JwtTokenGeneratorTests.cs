using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using WppSender.Domain;
using WppSender.Infrastructure.Security;
using Xunit;

namespace WppSender.Infrastructure.Tests;

public class JwtTokenGeneratorTests
{
    [Fact]
    public void DeveGerarTokenComEmailNoClaimEExpiracaoConfigurada()
    {
        var options = Options.Create(new JwtOptions
        {
            SigningKey = "chave-de-teste-com-pelo-menos-32-caracteres!!",
            ExpirationMinutes = 60
        });
        var gerador = new JwtTokenGenerator(options);
        var usuario = new Usuario(Guid.NewGuid(), "user@teste.com", "hash-qualquer");

        var token = gerador.GerarToken(usuario);

        var handler = new JwtSecurityTokenHandler();
        var tokenLido = handler.ReadJwtToken(token);
        Assert.Equal("user@teste.com", tokenLido.Claims.First(c => c.Type == "email").Value);
        Assert.True(tokenLido.ValidTo > DateTime.UtcNow.AddMinutes(59));
        Assert.True(tokenLido.ValidTo <= DateTime.UtcNow.AddMinutes(60).AddSeconds(5));
    }
}
