using WppSender.Application.Auth;
using WppSender.Application.Tests.Fakes;
using WppSender.Domain;
using Xunit;

namespace WppSender.Application.Tests;

public class AutenticarUsuarioUseCaseTests
{
    [Fact]
    public async Task DeveRetornarSucessoComToken_QuandoCredenciaisEstaoCorretas()
    {
        var repositorio = new FakeUsuarioRepository();
        var hasher = new FakePasswordHasher();
        await repositorio.AdicionarAsync(new Usuario(Guid.NewGuid(), "user@teste.com", hasher.Hash("senha123")));
        var useCase = new AutenticarUsuarioUseCase(repositorio, hasher, new FakeJwtTokenGenerator());

        var resultado = await useCase.ExecutarAsync("user@teste.com", "senha123");

        Assert.True(resultado.Sucesso);
        Assert.Equal("token-para-user@teste.com", resultado.Token);
        Assert.Null(resultado.MensagemErro);
    }

    [Fact]
    public async Task DeveRetornarFalhaComMensagemGenerica_QuandoEmailNaoExiste()
    {
        var useCase = new AutenticarUsuarioUseCase(new FakeUsuarioRepository(), new FakePasswordHasher(), new FakeJwtTokenGenerator());

        var resultado = await useCase.ExecutarAsync("naoexiste@teste.com", "qualquer");

        Assert.False(resultado.Sucesso);
        Assert.Equal("Email ou senha inválidos", resultado.MensagemErro);
        Assert.Null(resultado.Token);
    }

    [Fact]
    public async Task DeveRetornarFalhaComMensagemGenerica_QuandoSenhaEstaErrada()
    {
        var repositorio = new FakeUsuarioRepository();
        var hasher = new FakePasswordHasher();
        await repositorio.AdicionarAsync(new Usuario(Guid.NewGuid(), "user@teste.com", hasher.Hash("senhaCorreta")));
        var useCase = new AutenticarUsuarioUseCase(repositorio, hasher, new FakeJwtTokenGenerator());

        var resultado = await useCase.ExecutarAsync("user@teste.com", "senhaErrada");

        Assert.False(resultado.Sucesso);
        Assert.Equal("Email ou senha inválidos", resultado.MensagemErro);
        Assert.Null(resultado.Token);
    }
}
