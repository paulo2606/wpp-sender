using WppSender.Application.Grupos;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using WppSender.Domain;
using Xunit;

namespace WppSender.Application.Tests;

public class ListarGruposUseCaseTests
{
    [Fact]
    public async Task DeveListarComContagemDeLeadsAtivos()
    {
        var leadRepositorio = new FakeLeadRepository();
        var grupoRepositorio = new FakeGrupoRepository(leadRepositorio);
        var criarLeadUseCase = new CriarLeadUseCase(leadRepositorio);
        var grupo = new Grupo(Guid.NewGuid(), "Grupo A", null);
        await grupoRepositorio.AdicionarAsync(grupo);
        await criarLeadUseCase.ExecutarAsync("Lead1", "11911111111", null, null, null, grupo.Id);
        await criarLeadUseCase.ExecutarAsync("Lead2", "11922222222", null, null, null, grupo.Id);
        var useCase = new ListarGruposUseCase(grupoRepositorio);

        var resultado = await useCase.ExecutarAsync(1, 10);

        Assert.Equal(1, resultado.Total);
        Assert.Equal(2, resultado.Itens[0].QuantidadeLeads);
    }

    [Fact]
    public async Task DeveListarComContagemZero_QuandoGrupoSemLeads()
    {
        var leadRepositorio = new FakeLeadRepository();
        var grupoRepositorio = new FakeGrupoRepository(leadRepositorio);
        var grupo = new Grupo(Guid.NewGuid(), "Grupo Vazio", null);
        await grupoRepositorio.AdicionarAsync(grupo);
        var useCase = new ListarGruposUseCase(grupoRepositorio);

        var resultado = await useCase.ExecutarAsync(1, 10);

        Assert.Equal(0, resultado.Itens[0].QuantidadeLeads);
    }

    [Fact]
    public async Task DeveClamparPaginaETamanhoPagina_QuandoValoresInvalidos()
    {
        var grupoRepositorio = new FakeGrupoRepository();
        var useCase = new ListarGruposUseCase(grupoRepositorio);

        var resultado = await useCase.ExecutarAsync(0, 1000);

        Assert.Equal(1, resultado.Pagina);
        Assert.Equal(100, resultado.TamanhoPagina);
    }
}
