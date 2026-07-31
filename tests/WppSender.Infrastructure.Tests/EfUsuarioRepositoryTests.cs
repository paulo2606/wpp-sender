using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using WppSender.Domain;
using WppSender.Infrastructure.Persistence;
using Xunit;

namespace WppSender.Infrastructure.Tests;

public class EfUsuarioRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16")
        .Build();

    private WppSenderDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<WppSenderDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .Options;

        _dbContext = new WppSenderDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _container.DisposeAsync();
    }

    [Fact]
    public async Task DeveEncontrarUsuario_QuandoEmailExisteNaBase()
    {
        var repositorio = new EfUsuarioRepository(_dbContext);
        var usuario = new Usuario(Guid.NewGuid(), "existente@teste.com", "hash-qualquer");
        await repositorio.AdicionarAsync(usuario);

        var encontrado = await repositorio.BuscarPorEmailAsync("existente@teste.com");

        Assert.NotNull(encontrado);
        Assert.Equal(usuario.Id, encontrado!.Id);
        Assert.Equal("existente@teste.com", encontrado.Email);
    }

    [Fact]
    public async Task DeveRetornarNulo_QuandoEmailNaoExisteNaBase()
    {
        var repositorio = new EfUsuarioRepository(_dbContext);
        await repositorio.AdicionarAsync(new Usuario(Guid.NewGuid(), "outro-usuario@teste.com", "hash-qualquer"));

        var encontrado = await repositorio.BuscarPorEmailAsync("naoexiste@teste.com");

        Assert.Null(encontrado);
    }
}
