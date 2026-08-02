using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using WppSender.Domain;
using WppSender.Infrastructure.Persistence;
using Xunit;

namespace WppSender.Infrastructure.Tests;

public class EfSessaoWhatsAppRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16").Build();
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
    public async Task ObterAsync_DeveRetornarLinhaSeedadaComoDesconectado()
    {
        var repositorio = new EfSessaoWhatsAppRepository(_dbContext);

        var sessao = await repositorio.ObterAsync();

        Assert.Equal(StatusSessaoWhatsApp.Desconectado, sessao.Status);
    }

    [Fact]
    public async Task AtualizarAsync_DevePersistirNovoStatus()
    {
        var repositorio = new EfSessaoWhatsAppRepository(_dbContext);
        var sessao = await repositorio.ObterAsync();

        sessao.MarcarConectado();
        await repositorio.AtualizarAsync(sessao);

        await using var novoDbContext = new WppSenderDbContext(new DbContextOptionsBuilder<WppSenderDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .Options);
        var repositorio2 = new EfSessaoWhatsAppRepository(novoDbContext);
        var sessaoRecarregada = await repositorio2.ObterAsync();
        Assert.Equal(StatusSessaoWhatsApp.Conectado, sessaoRecarregada.Status);
    }
}
