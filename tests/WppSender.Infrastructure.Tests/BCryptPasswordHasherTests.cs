using WppSender.Infrastructure.Security;
using Xunit;

namespace WppSender.Infrastructure.Tests;

public class BCryptPasswordHasherTests
{
    [Fact]
    public void DeveVerificarComoValido_QuandoSenhaPlanaCorrespondeAoHashGerado()
    {
        var hasher = new BCryptPasswordHasher();
        var hash = hasher.Hash("minhaSenha123");

        var resultado = hasher.Verify("minhaSenha123", hash);

        Assert.True(resultado);
    }

    [Fact]
    public void DeveVerificarComoInvalido_QuandoSenhaPlanaNaoCorrespondeAoHash()
    {
        var hasher = new BCryptPasswordHasher();
        var hash = hasher.Hash("minhaSenha123");

        var resultado = hasher.Verify("senhaErrada", hash);

        Assert.False(resultado);
    }
}
