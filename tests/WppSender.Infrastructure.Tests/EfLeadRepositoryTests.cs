using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using WppSender.Domain;
using WppSender.Infrastructure.Persistence;
using Xunit;

namespace WppSender.Infrastructure.Tests;

public class EfLeadRepositoryTests : IAsyncLifetime
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
    public async Task BuscarPorTelefoneNormalizadoAsync_NaoDeveEncontrar_QuandoLeadComEsseTelefoneEstaExcluido()
    {
        var repositorio = new EfLeadRepository(_dbContext);
        var lead = new Lead(Guid.NewGuid(), "Fulano", "11912345678", null, null, null);
        await repositorio.AdicionarAsync(lead);
        lead.Excluir();
        await repositorio.AtualizarAsync(lead);

        var encontrado = await repositorio.BuscarPorTelefoneNormalizadoAsync("11912345678");

        Assert.Null(encontrado);
    }

    [Fact]
    public async Task DevePermitirNovoCadastroComMesmoTelefone_DepoisDeExcluirOAnterior_NoNivelDoBanco()
    {
        var repositorio = new EfLeadRepository(_dbContext);
        var leadAntigo = new Lead(Guid.NewGuid(), "Antigo", "11912345678", null, null, null);
        await repositorio.AdicionarAsync(leadAntigo);
        leadAntigo.Excluir();
        await repositorio.AtualizarAsync(leadAntigo);

        var leadNovo = new Lead(Guid.NewGuid(), "Novo", "11912345678", null, null, null);
        await repositorio.AdicionarAsync(leadNovo);

        var encontrado = await repositorio.BuscarPorTelefoneNormalizadoAsync("11912345678");
        Assert.NotNull(encontrado);
        Assert.Equal("Novo", encontrado!.Nome);
    }

    [Fact]
    public async Task ListarAsync_NaoDeveRetornarLeadsExcluidos()
    {
        var repositorio = new EfLeadRepository(_dbContext);
        var ativo = new Lead(Guid.NewGuid(), "Ativo", "11911111111", null, null, null);
        var excluido = new Lead(Guid.NewGuid(), "Excluido", "11922222222", null, null, null);
        await repositorio.AdicionarAsync(ativo);
        await repositorio.AdicionarAsync(excluido);
        excluido.Excluir();
        await repositorio.AtualizarAsync(excluido);

        var (itens, total) = await repositorio.ListarAsync(busca: null, pagina: 1, tamanhoPagina: 10);

        Assert.Equal(1, total);
        Assert.Single(itens);
        Assert.Equal("Ativo", itens[0].Nome);
    }

    [Fact]
    public async Task ListarAsync_DeveRespeitarPaginacao()
    {
        var repositorio = new EfLeadRepository(_dbContext);
        for (var i = 0; i < 5; i++)
        {
            await repositorio.AdicionarAsync(new Lead(Guid.NewGuid(), $"Lead{i}", $"1191111000{i}", null, null, null));
        }

        var (itens, total) = await repositorio.ListarAsync(busca: null, pagina: 1, tamanhoPagina: 2);

        Assert.Equal(5, total);
        Assert.Equal(2, itens.Count);
    }

    [Fact]
    public async Task ListarAsync_NaoDeveDuplicarNemPularLeads_QuandoVariosCompartilhamOMesmoNomeEPaginados()
    {
        var repositorio = new EfLeadRepository(_dbContext);
        var idsEsperados = new List<Guid>();
        for (var i = 0; i < 6; i++)
        {
            var lead = new Lead(Guid.NewGuid(), "MesmoNome", $"1190000{i:D4}", null, null, null);
            idsEsperados.Add(lead.Id);
            await repositorio.AdicionarAsync(lead);
        }

        var idsColetados = new List<Guid>();
        var pagina = 1;
        const int tamanhoPagina = 2;
        while (true)
        {
            var (itens, total) = await repositorio.ListarAsync(busca: null, pagina, tamanhoPagina);
            if (itens.Count == 0)
            {
                break;
            }

            idsColetados.AddRange(itens.Select(l => l.Id));

            if (pagina * tamanhoPagina >= total)
            {
                break;
            }

            pagina++;
        }

        Assert.Equal(idsEsperados.OrderBy(id => id), idsColetados.OrderBy(id => id));
        Assert.Equal(idsEsperados.Count, idsColetados.Distinct().Count());
    }
}
