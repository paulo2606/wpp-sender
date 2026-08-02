using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using WppSender.Infrastructure.Persistence;
using Xunit;

namespace WppSender.Infrastructure.Tests;

public class EfConfiguracaoEnvioRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16").Build();
    private string _connectionString = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _connectionString = _container.GetConnectionString();

        var options = new DbContextOptionsBuilder<WppSenderDbContext>()
            .UseNpgsql(_connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var dbContext = new WppSenderDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Database.ExecuteSqlRawAsync(
            "UPDATE configuracao_envio SET limite_diario_envios = 1, envios_realizados_hoje = 0, data_referencia = CURRENT_DATE");
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    private WppSenderDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<WppSenderDbContext>()
            .UseNpgsql(_connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new WppSenderDbContext(options);
    }

    [Fact]
    public async Task TentarRegistrarEnvioAsync_DeveSerializarDuasChamadasConcorrentes_ComExatamenteUmSucesso()
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        await using var dbContext1 = CriarDbContext();
        await using var dbContext2 = CriarDbContext();
        var repositorio1 = new EfConfiguracaoEnvioRepository(dbContext1);
        var repositorio2 = new EfConfiguracaoEnvioRepository(dbContext2);

        var tarefa1 = repositorio1.TentarRegistrarEnvioAsync(hoje);
        var tarefa2 = repositorio2.TentarRegistrarEnvioAsync(hoje);
        var resultados = await Task.WhenAll(tarefa1, tarefa2);

        Assert.Equal(1, resultados.Count(r => r));
        Assert.Equal(1, resultados.Count(r => !r));
    }

    [Fact]
    public async Task TentarRegistrarEnvioAsync_DeveResetarContador_QuandoDataReferenciaEhAntiga()
    {
        await using var setup = CriarDbContext();
        await setup.Database.ExecuteSqlRawAsync(
            "UPDATE configuracao_envio SET limite_diario_envios = 5, envios_realizados_hoje = 5, data_referencia = CURRENT_DATE - INTERVAL '1 day'");

        await using var dbContext = CriarDbContext();
        var repositorio = new EfConfiguracaoEnvioRepository(dbContext);

        var resultado = await repositorio.TentarRegistrarEnvioAsync(DateOnly.FromDateTime(DateTime.UtcNow));

        Assert.True(resultado);
    }
}
