using WppSender.Application.Dashboard;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using Xunit;

namespace WppSender.Application.Tests;

public class ContarLeadsRecentesUseCaseTests
{
    [Fact]
    public async Task DeveExcluirLeadsSoftDeletados()
    {
        var leadRepositorio = new FakeLeadRepository();
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Recente", "11911111111", null, null, null);
        var excluido = await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Excluido", "11922222222", null, null, null);
        await new ExcluirLeadUseCase(leadRepositorio).ExecutarAsync(excluido.LeadId!.Value);
        var useCase = new ContarLeadsRecentesUseCase(leadRepositorio);

        var resultado = await useCase.ExecutarAsync(dias: 7);

        Assert.Equal(1, resultado);
    }

    [Fact]
    public async Task DeveUsarSeteDiasComoPadrao_QuandoNaoInformado()
    {
        var leadRepositorio = new FakeLeadRepository();
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead1", "11911111111", null, null, null);
        var useCase = new ContarLeadsRecentesUseCase(leadRepositorio);

        var resultado = await useCase.ExecutarAsync();

        Assert.Equal(1, resultado);
    }
}
