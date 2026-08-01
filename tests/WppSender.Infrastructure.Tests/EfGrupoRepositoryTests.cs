using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using WppSender.Domain;
using WppSender.Infrastructure.Persistence;
using Xunit;

namespace WppSender.Infrastructure.Tests;

public class EfGrupoRepositoryTests : IAsyncLifetime
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
    public async Task ListarComContagemAsync_DeveContarApenasLeadsAtivos()
    {
        var grupoRepositorio = new EfGrupoRepository(_dbContext);
        var leadRepositorio = new EfLeadRepository(_dbContext);
        var grupo = new Grupo(Guid.NewGuid(), "Grupo A", null);
        await grupoRepositorio.AdicionarAsync(grupo);
        var leadAtivo = new Lead(Guid.NewGuid(), "Ativo", "11911111111", null, null, null, grupo.Id);
        var leadExcluido = new Lead(Guid.NewGuid(), "Excluido", "11922222222", null, null, null, grupo.Id);
        await leadRepositorio.AdicionarAsync(leadAtivo);
        await leadRepositorio.AdicionarAsync(leadExcluido);
        leadExcluido.Excluir();
        await leadRepositorio.AtualizarAsync(leadExcluido);

        var (itens, total) = await grupoRepositorio.ListarComContagemAsync(1, 10);

        Assert.Equal(1, total);
        Assert.Equal(1, itens[0].QuantidadeLeads);
    }

    [Fact]
    public async Task ExcluirGrupo_DeveDesvincularLeadsAutomaticamente_ViaOnDeleteSetNull()
    {
        var grupoRepositorio = new EfGrupoRepository(_dbContext);
        var leadRepositorio = new EfLeadRepository(_dbContext);
        var grupo = new Grupo(Guid.NewGuid(), "Grupo Temporario", null);
        await grupoRepositorio.AdicionarAsync(grupo);
        var lead = new Lead(Guid.NewGuid(), "Fulano", "11911111111", null, null, null, grupo.Id);
        await leadRepositorio.AdicionarAsync(lead);

        await grupoRepositorio.RemoverAsync(grupo);

        var leadRecarregado = await leadRepositorio.BuscarPorIdAsync(lead.Id);
        Assert.NotNull(leadRecarregado);
        Assert.Null(leadRecarregado!.GrupoId);
    }

    [Fact]
    public async Task ExecutarTransacaoAsync_DeveReverterMudancas_QuandoOperacaoLancaExcecao()
    {
        var unitOfWork = new EfUnitOfWork(_dbContext);
        var grupoRepositorio = new EfGrupoRepository(_dbContext);
        var grupoId = Guid.NewGuid();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await unitOfWork.ExecutarTransacaoAsync(async () =>
            {
                await grupoRepositorio.AdicionarAsync(new Grupo(grupoId, "Temporario", null));
                throw new InvalidOperationException("Falha simulada");
            });
        });

        var encontrado = await grupoRepositorio.BuscarPorIdAsync(grupoId);
        Assert.Null(encontrado);
    }
}
