using WppSender.Application.Campanhas;
using WppSender.Application.Dashboard;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using Xunit;

namespace WppSender.Application.Tests;

public class ObterProximaCampanhaAgendadaUseCaseTests
{
    [Fact]
    public async Task DeveRetornarAMaisProximaPorData()
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var grupoId = Guid.NewGuid();
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead1", "11911111111", null, null, null, grupoId);
        var criarUseCase = new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork());
        await criarUseCase.ExecutarAsync("Distante", "Msg", grupoId, DateTime.UtcNow.AddDays(10));
        await criarUseCase.ExecutarAsync("Proxima", "Msg", grupoId, DateTime.UtcNow.AddDays(1));
        var useCase = new ObterProximaCampanhaAgendadaUseCase(campanhaRepositorio);

        var resultado = await useCase.ExecutarAsync();

        Assert.NotNull(resultado);
        Assert.Equal("Proxima", resultado!.Nome);
    }

    [Fact]
    public async Task DeveRetornarNulo_QuandoNaoHaCampanhasAgendadas()
    {
        var campanhaRepositorio = new FakeCampanhaRepository();
        var useCase = new ObterProximaCampanhaAgendadaUseCase(campanhaRepositorio);

        var resultado = await useCase.ExecutarAsync();

        Assert.Null(resultado);
    }
}
