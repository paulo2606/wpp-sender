using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using WppSender.Domain;
using WppSender.Infrastructure.Persistence;
using Xunit;

namespace WppSender.Infrastructure.Tests;

public class EfEnvioRepositoryTests : IAsyncLifetime
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
    public async Task BuscarProximoPendenteAsync_DeveRetornarSoPendenteDaquelaCampanha()
    {
        var campanhaRepositorio = new EfCampanhaRepository(_dbContext);
        var envioRepositorio = new EfEnvioRepository(_dbContext);
        var campanha = new Campanha(Guid.NewGuid(), "Campanha", "Msg", Guid.NewGuid(), null);
        await campanhaRepositorio.AdicionarAsync(campanha);
        var pendente = new Envio(Guid.NewGuid(), campanha.Id, Guid.NewGuid());
        var enviado = new Envio(Guid.NewGuid(), campanha.Id, Guid.NewGuid());
        enviado.MarcarComoEnviado(DateTime.UtcNow, "wamid.1");
        await envioRepositorio.AdicionarVariosAsync(new[] { pendente, enviado });

        var resultado = await envioRepositorio.BuscarProximoPendenteAsync(campanha.Id);

        Assert.NotNull(resultado);
        Assert.Equal(pendente.Id, resultado!.Id);
    }

    [Fact]
    public async Task ContarPorStatusAsync_DeveAgruparCorretamente()
    {
        var campanhaRepositorio = new EfCampanhaRepository(_dbContext);
        var envioRepositorio = new EfEnvioRepository(_dbContext);
        var campanha = new Campanha(Guid.NewGuid(), "Campanha", "Msg", Guid.NewGuid(), null);
        await campanhaRepositorio.AdicionarAsync(campanha);
        var enviado = new Envio(Guid.NewGuid(), campanha.Id, Guid.NewGuid());
        enviado.MarcarComoEnviado(DateTime.UtcNow, "wamid.1");
        var falhou = new Envio(Guid.NewGuid(), campanha.Id, Guid.NewGuid());
        falhou.MarcarComoFalhou("erro");
        var pendente = new Envio(Guid.NewGuid(), campanha.Id, Guid.NewGuid());
        await envioRepositorio.AdicionarVariosAsync(new[] { enviado, falhou, pendente });

        var contagens = await envioRepositorio.ContarPorStatusAsync(campanha.Id);

        Assert.Equal(1, contagens[StatusEnvio.Enviado]);
        Assert.Equal(1, contagens[StatusEnvio.Falhou]);
        Assert.Equal(1, contagens[StatusEnvio.Pendente]);
    }

    [Fact]
    public async Task ListarAguardandoConfirmacaoAsync_DeveIgnorarEnviosDeCampanhaCancelada()
    {
        var campanhaRepositorio = new EfCampanhaRepository(_dbContext);
        var envioRepositorio = new EfEnvioRepository(_dbContext);

        var campanhaEmAndamento = new Campanha(Guid.NewGuid(), "Em andamento", "Msg", Guid.NewGuid(), null);
        campanhaEmAndamento.Iniciar();
        await campanhaRepositorio.AdicionarAsync(campanhaEmAndamento);
        var envioRastreado = new Envio(Guid.NewGuid(), campanhaEmAndamento.Id, Guid.NewGuid());
        envioRastreado.MarcarComoEnviado(DateTime.UtcNow, "wamid.rastreado");

        var campanhaCancelada = new Campanha(Guid.NewGuid(), "Cancelada", "Msg", Guid.NewGuid(), null);
        campanhaCancelada.Iniciar();
        campanhaCancelada.Pausar();
        campanhaCancelada.Cancelar();
        await campanhaRepositorio.AdicionarAsync(campanhaCancelada);
        var envioCongelado = new Envio(Guid.NewGuid(), campanhaCancelada.Id, Guid.NewGuid());
        envioCongelado.MarcarComoEnviado(DateTime.UtcNow, "wamid.congelado");

        await envioRepositorio.AdicionarVariosAsync(new[] { envioRastreado, envioCongelado });

        var aguardando = await envioRepositorio.ListarAguardandoConfirmacaoAsync();

        Assert.Contains(aguardando, e => e.Id == envioRastreado.Id);
        Assert.DoesNotContain(aguardando, e => e.Id == envioCongelado.Id);
    }
}
