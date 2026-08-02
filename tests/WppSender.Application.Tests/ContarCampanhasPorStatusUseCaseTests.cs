using WppSender.Application.Campanhas;
using WppSender.Application.Dashboard;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using Xunit;

namespace WppSender.Application.Tests;

public class ContarCampanhasPorStatusUseCaseTests
{
    [Fact]
    public async Task DeveContarCampanhasAgrupadasPorStatus()
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var grupoId = Guid.NewGuid();
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead1", "11911111111", null, null, null, grupoId);
        var criarCampanhaUseCase = new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork());
        await criarCampanhaUseCase.ExecutarAsync("Rascunho1", "Msg", grupoId, null);
        await criarCampanhaUseCase.ExecutarAsync("Rascunho2", "Msg", grupoId, null);
        await criarCampanhaUseCase.ExecutarAsync("Agendada1", "Msg", grupoId, DateTime.UtcNow.AddDays(1));
        var useCase = new ContarCampanhasPorStatusUseCase(campanhaRepositorio);

        var resultado = await useCase.ExecutarAsync();

        Assert.Equal(2, resultado.Rascunho);
        Assert.Equal(1, resultado.Agendada);
        Assert.Equal(0, resultado.EmAndamento);
        Assert.Equal(0, resultado.Pausada);
        Assert.Equal(0, resultado.Concluida);
    }

    [Fact]
    public async Task DeveRetornarTudoZero_QuandoNaoHaCampanhas()
    {
        var campanhaRepositorio = new FakeCampanhaRepository();
        var useCase = new ContarCampanhasPorStatusUseCase(campanhaRepositorio);

        var resultado = await useCase.ExecutarAsync();

        Assert.Equal(0, resultado.Rascunho);
        Assert.Equal(0, resultado.Concluida);
    }
}
