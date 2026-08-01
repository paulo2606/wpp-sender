using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using Xunit;

namespace WppSender.Application.Tests;

public class ListarLeadsUseCaseTests
{
    [Fact]
    public async Task DeveListarTodosOsAtivos_QuandoSemBusca()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var listarUseCase = new ListarLeadsUseCase(repositorio);
        await criarUseCase.ExecutarAsync("Ana", "11911111111", null, null, null);
        await criarUseCase.ExecutarAsync("Bruno", "11922222222", null, null, null);

        var resultado = await listarUseCase.ExecutarAsync(null, 1, 10);

        Assert.Equal(2, resultado.Total);
        Assert.Equal(2, resultado.Itens.Count);
    }

    [Fact]
    public async Task DeveFiltrarPorBuscaDeNome()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var listarUseCase = new ListarLeadsUseCase(repositorio);
        await criarUseCase.ExecutarAsync("Ana Paula", "11911111111", null, null, null);
        await criarUseCase.ExecutarAsync("Bruno", "11922222222", null, null, null);

        var resultado = await listarUseCase.ExecutarAsync("Ana", 1, 10);

        Assert.Equal(1, resultado.Total);
        Assert.Equal("Ana Paula", resultado.Itens[0].Nome);
    }

    [Fact]
    public async Task DeveRespeitarPaginacao()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var listarUseCase = new ListarLeadsUseCase(repositorio);
        for (var i = 0; i < 3; i++)
        {
            await criarUseCase.ExecutarAsync($"Lead{i}", $"1191111000{i}", null, null, null);
        }

        var resultado = await listarUseCase.ExecutarAsync(null, 1, 2);

        Assert.Equal(3, resultado.Total);
        Assert.Equal(2, resultado.Itens.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task DeveClamparPaginaParaOMinimoDeUm_QuandoPaginaZeroOuNegativa(int paginaInvalida)
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var listarUseCase = new ListarLeadsUseCase(repositorio);
        await criarUseCase.ExecutarAsync("Ana", "11911111111", null, null, null);
        await criarUseCase.ExecutarAsync("Bruno", "11922222222", null, null, null);

        var resultadoPaginaInvalida = await listarUseCase.ExecutarAsync(null, paginaInvalida, 10);
        var resultadoPaginaUm = await listarUseCase.ExecutarAsync(null, 1, 10);

        Assert.Equal(resultadoPaginaUm.Itens.Select(i => i.Id), resultadoPaginaInvalida.Itens.Select(i => i.Id));
        Assert.Equal(resultadoPaginaUm.Total, resultadoPaginaInvalida.Total);
    }

    [Fact]
    public async Task DeveClamparTamanhoDaPaginaParaCemNoMaximo_QuandoTamanhoOversized()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var listarUseCase = new ListarLeadsUseCase(repositorio);
        for (var i = 0; i < 110; i++)
        {
            await criarUseCase.ExecutarAsync($"Lead{i:D3}", $"119111{i:D5}", null, null, null);
        }

        var resultadoOversized = await listarUseCase.ExecutarAsync(null, 1, 10_000);

        Assert.Equal(110, resultadoOversized.Total);
        Assert.Equal(100, resultadoOversized.Itens.Count);
    }
}
