using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace WppSender.Api.IntegrationTests;

public class AuthEndpointTests : IAsyncLifetime
{
    private const string Email = "admin@teste.com";
    private const string Senha = "senhaAdminDeTeste123";

    private WppSenderApiFactory _factory = null!;

    public async Task InitializeAsync()
    {
        _factory = new WppSenderApiFactory();
        await _factory.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task DeveRetornar401_QuandoChamaEndpointProtegidoSemToken()
    {
        var client = _factory.CreateClient();

        var resposta = await client.GetAsync("/api/health/privado");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task DeveRegistrarELogarComSucesso_QuandoBaseEstaVazia()
    {
        var client = _factory.CreateClient();

        var respostaRegistro = await client.PostAsJsonAsync("/api/auth/registrar", new { email = Email, senha = Senha });
        Assert.Equal(HttpStatusCode.OK, respostaRegistro.StatusCode);

        var respostaLogin = await client.PostAsJsonAsync("/api/auth/login", new { email = Email, senha = Senha });
        Assert.Equal(HttpStatusCode.OK, respostaLogin.StatusCode);
        var corpo = await respostaLogin.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.False(string.IsNullOrWhiteSpace(corpo!["token"]));
    }

    [Fact]
    public async Task DeveRetornarConflito_QuandoTentaRegistrarDeNovoAposJaExistirUsuario()
    {
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/registrar", new { email = Email, senha = Senha });

        var segundaTentativa = await client.PostAsJsonAsync("/api/auth/registrar", new { email = "outro@teste.com", senha = "outraSenha123" });

        Assert.Equal(HttpStatusCode.Conflict, segundaTentativa.StatusCode);
    }

    [Fact]
    public async Task DeveRetornar401ComMensagemClara_QuandoSenhaEstaErrada()
    {
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/auth/registrar", new { email = Email, senha = Senha });

        var resposta = await client.PostAsJsonAsync("/api/auth/login", new { email = Email, senha = "senhaErrada" });

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        var corpo = await resposta.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.Equal("Email ou senha inválidos", corpo!["message"]);
    }
}
