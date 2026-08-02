using WppSender.Application.Dashboard;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using Xunit;

namespace WppSender.Application.Tests;

public class ContarLeadsPorGrupoUseCaseTests
{
    [Fact]
    public async Task DeveContarLeadsAtivosAgrupadosPorGrupoComNome()
    {
        var leadRepositorio = new FakeLeadRepository();
        var grupoRepositorio = new FakeGrupoRepository();
        var grupo = new WppSender.Domain.Grupo(Guid.NewGuid(), "Clientes VIP", null);
        await grupoRepositorio.AdicionarAsync(grupo);
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead1", "11911111111", null, null, null, grupo.Id);
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead2", "11922222222", null, null, null, grupo.Id);
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("SemGrupo", "11933333333", null, null, null);
        var useCase = new ContarLeadsPorGrupoUseCase(leadRepositorio, grupoRepositorio);

        var resultado = await useCase.ExecutarAsync();

        Assert.Single(resultado);
        Assert.Equal("Clientes VIP", resultado[0].NomeGrupo);
        Assert.Equal(2, resultado[0].Quantidade);
    }

    [Fact]
    public async Task DeveRetornarListaVazia_QuandoNaoHaLeadsEmGrupos()
    {
        var leadRepositorio = new FakeLeadRepository();
        var grupoRepositorio = new FakeGrupoRepository();
        var useCase = new ContarLeadsPorGrupoUseCase(leadRepositorio, grupoRepositorio);

        var resultado = await useCase.ExecutarAsync();

        Assert.Empty(resultado);
    }
}
