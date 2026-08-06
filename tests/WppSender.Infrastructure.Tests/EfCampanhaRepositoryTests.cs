using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using WppSender.Domain;
using WppSender.Infrastructure.Persistence;
using Xunit;

namespace WppSender.Infrastructure.Tests;

public class EfCampanhaRepositoryTests : IAsyncLifetime
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
    public async Task ListarAgendadasParaIniciarAsync_DeveRetornarSoAgendadasComHorarioVencido()
    {
        var repositorio = new EfCampanhaRepository(_dbContext);
        var agora = new DateTime(2026, 8, 2, 12, 0, 0, DateTimeKind.Utc);
        var vencida = new Campanha(Guid.NewGuid(), "Vencida", "Msg", Guid.NewGuid(), agora.AddMinutes(-5));
        var futura = new Campanha(Guid.NewGuid(), "Futura", "Msg", Guid.NewGuid(), agora.AddMinutes(5));
        var rascunho = new Campanha(Guid.NewGuid(), "Rascunho", "Msg", Guid.NewGuid(), null);
        await repositorio.AdicionarAsync(vencida);
        await repositorio.AdicionarAsync(futura);
        await repositorio.AdicionarAsync(rascunho);

        var resultado = await repositorio.ListarAgendadasParaIniciarAsync(agora);

        Assert.Single(resultado);
        Assert.Equal(vencida.Id, resultado[0].Id);
    }

    [Fact]
    public async Task RemoverAsync_DeveApagarEnviosEmCascata()
    {

        var campanhaRepositorio = new EfCampanhaRepository(_dbContext);
        var envioRepositorio = new EfEnvioRepository(_dbContext);
        var campanha = new Campanha(Guid.NewGuid(), "Campanha", "Msg", Guid.NewGuid(), null);
        await campanhaRepositorio.AdicionarAsync(campanha);
        var envio = new Envio(Guid.NewGuid(), campanha.Id, Guid.NewGuid());
        await envioRepositorio.AdicionarVariosAsync(new[] { envio });

        await campanhaRepositorio.RemoverAsync(campanha);

        var contagens = await envioRepositorio.ContarPorStatusAsync(campanha.Id);
        Assert.Empty(contagens);
    }

    [Fact]
    public async Task ListarAsync_DeveFiltrarPorStatus_QuandoInformado()
    {
        var repositorio = new EfCampanhaRepository(_dbContext);
        var rascunho = new Campanha(Guid.NewGuid(), "Rascunho", "Msg", Guid.NewGuid(), null);
        var agendada = new Campanha(Guid.NewGuid(), "Agendada", "Msg", Guid.NewGuid(), DateTime.UtcNow.AddDays(1));
        await repositorio.AdicionarAsync(rascunho);
        await repositorio.AdicionarAsync(agendada);

        var (itens, total) = await repositorio.ListarAsync(StatusCampanha.Rascunho, 1, 10);

        Assert.Equal(1, total);
        Assert.Equal("Rascunho", itens[0].Nome);
    }
}
