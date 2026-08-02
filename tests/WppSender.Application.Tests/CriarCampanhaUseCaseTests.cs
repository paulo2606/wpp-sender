using WppSender.Application.Campanhas;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using Xunit;

namespace WppSender.Application.Tests;

public class CriarCampanhaUseCaseTests
{
    [Fact]
    public async Task DeveCriarComStatusRascunhoEGerarEnviosPendentes_QuandoGrupoTemLeadsAtivos()
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var criarLeadUseCase = new CriarLeadUseCase(leadRepositorio);
        var grupoId = Guid.NewGuid();
        var lead1 = await criarLeadUseCase.ExecutarAsync("Lead1", "11911111111", null, null, null, grupoId);
        var lead2 = await criarLeadUseCase.ExecutarAsync("Lead2", "11922222222", null, null, null, grupoId);
        var useCase = new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork());

        var resultado = await useCase.ExecutarAsync("Campanha", "Olá {{nome}}", grupoId, agendadoPara: null);

        Assert.True(resultado.Sucesso);
        var campanha = await campanhaRepositorio.BuscarPorIdAsync(resultado.CampanhaId!.Value);
        Assert.NotNull(campanha);
        var contagens = await envioRepositorio.ContarPorStatusAsync(campanha!.Id);
        Assert.Equal(2, contagens[WppSender.Domain.StatusEnvio.Pendente]);
    }

    [Fact]
    public async Task DeveRetornarFalha_QuandoGrupoSemLeadsAtivos()
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var useCase = new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork());

        var resultado = await useCase.ExecutarAsync("Campanha", "Msg", Guid.NewGuid(), agendadoPara: null);

        Assert.False(resultado.Sucesso);
        var (_, total) = await campanhaRepositorio.ListarAsync(null, 1, 10);
        Assert.Equal(0, total);
    }

    [Fact]
    public async Task DeveCriarComStatusAgendada_QuandoAgendadoParaInformado()
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var criarLeadUseCase = new CriarLeadUseCase(leadRepositorio);
        var grupoId = Guid.NewGuid();
        await criarLeadUseCase.ExecutarAsync("Lead1", "11911111111", null, null, null, grupoId);
        var useCase = new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork());

        var resultado = await useCase.ExecutarAsync("Campanha", "Msg", grupoId, DateTime.UtcNow.AddDays(1));

        Assert.True(resultado.Sucesso);
        var campanha = await campanhaRepositorio.BuscarPorIdAsync(resultado.CampanhaId!.Value);
        Assert.Equal(WppSender.Domain.StatusCampanha.Agendada, campanha!.Status);
    }
}
