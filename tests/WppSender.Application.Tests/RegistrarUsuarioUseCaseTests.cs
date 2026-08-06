using WppSender.Application.Auth;
using WppSender.Application.Tests.Fakes;
using WppSender.Domain;
using Xunit;

namespace WppSender.Application.Tests;

public class RegistrarUsuarioUseCaseTests
{
    [Fact]
    public async Task DeveRegistrarComSucesso_QuandoNaoExisteNenhumUsuarioNaBase()
    {
        var repositorio = new FakeUsuarioRepository();
        var useCase = new RegistrarUsuarioUseCase(repositorio, new FakePasswordHasher());

        var resultado = await useCase.ExecutarAsync("admin@teste.com", "senha123");

        Assert.True(resultado.Sucesso);
        Assert.Null(resultado.MensagemErro);
        var criado = await repositorio.BuscarPorEmailAsync("admin@teste.com");
        Assert.NotNull(criado);
    }

    [Fact]
    public async Task DeveRetornarFalha_QuandoJaExisteUsuarioCadastrado()
    {
        var repositorio = new FakeUsuarioRepository();
        var hasher = new FakePasswordHasher();
        await repositorio.AdicionarAsync(new Usuario(Guid.NewGuid(), "existente@teste.com", hasher.Hash("outraSenha")));
        var useCase = new RegistrarUsuarioUseCase(repositorio, hasher);

        var resultado = await useCase.ExecutarAsync("segundo@teste.com", "senha123");

        Assert.False(resultado.Sucesso);
        Assert.Equal("Cadastro indisponível", resultado.MensagemErro);
        var naoDeveExistir = await repositorio.BuscarPorEmailAsync("segundo@teste.com");
        Assert.Null(naoDeveExistir);
    }

    [Fact]
    public async Task DeveConseguirAutenticar_ComASenhaUsadaNoRegistro()
    {
        var repositorio = new FakeUsuarioRepository();
        var hasher = new FakePasswordHasher();
        var registrarUseCase = new RegistrarUsuarioUseCase(repositorio, hasher);
        var autenticarUseCase = new AutenticarUsuarioUseCase(repositorio, hasher, new FakeJwtTokenGenerator());
        await registrarUseCase.ExecutarAsync("admin@teste.com", "senha123");

        var resultado = await autenticarUseCase.ExecutarAsync("admin@teste.com", "senha123");

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Valor);
    }
}
