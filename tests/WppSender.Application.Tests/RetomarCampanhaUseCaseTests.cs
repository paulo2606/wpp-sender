using WppSender.Application.Campanhas;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using WppSender.Domain;
using Xunit;

namespace WppSender.Application.Tests;

public class RetomarCampanhaUseCaseTests
{
    private static async Task<(FakeCampanhaRepository CampanhaRepositorio, FakeSessaoWhatsAppRepository SessaoRepositorio, FakeCampanhaJobScheduler Scheduler, Guid CampanhaId)> CriarCenarioPausadoAsync()
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var grupoId = Guid.NewGuid();
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead1", "11911111111", null, null, null, grupoId);
        var criada = await new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork())
            .ExecutarAsync("Campanha", "Msg", grupoId, null);
        var campanha = await campanhaRepositorio.BuscarPorIdAsync(criada.Valor);
        campanha!.Iniciar();
        campanha.Pausar();
        await campanhaRepositorio.AtualizarAsync(campanha);
        var sessaoRepositorio = new FakeSessaoWhatsAppRepository();
        var scheduler = new FakeCampanhaJobScheduler();

        return (campanhaRepositorio, sessaoRepositorio, scheduler, criada.Valor);
    }

    [Fact]
    public async Task DeveRetomarEAgendarProximoPasso_QuandoSessaoConectada()
    {
        var (campanhaRepositorio, sessaoRepositorio, scheduler, campanhaId) = await CriarCenarioPausadoAsync();
        await sessaoRepositorio.AtualizarAsync(new SessaoWhatsApp(StatusSessaoWhatsApp.Conectado));
        var useCase = new RetomarCampanhaUseCase(campanhaRepositorio, sessaoRepositorio, scheduler);

        var resultado = await useCase.ExecutarAsync(campanhaId);

        Assert.True(resultado.Sucesso);
        var campanha = await campanhaRepositorio.BuscarPorIdAsync(campanhaId);
        Assert.Equal(StatusCampanha.EmAndamento, campanha!.Status);
        Assert.Single(scheduler.Agendamentos);
    }

    [Fact]
    public async Task DeveRetornarFalhaComErroSessaoDesconectada_QuandoSessaoNaoConectada()
    {
        var (campanhaRepositorio, sessaoRepositorio, scheduler, campanhaId) = await CriarCenarioPausadoAsync();
        var useCase = new RetomarCampanhaUseCase(campanhaRepositorio, sessaoRepositorio, scheduler);

        var resultado = await useCase.ExecutarAsync(campanhaId);

        Assert.False(resultado.Sucesso);
        Assert.Equal(RetomarCampanhaErro.SessaoDesconectada, resultado.Erro);
        var campanha = await campanhaRepositorio.BuscarPorIdAsync(campanhaId);
        Assert.Equal(StatusCampanha.Pausada, campanha!.Status);
        Assert.Empty(scheduler.Agendamentos);
    }

    [Fact]
    public async Task DeveRetornarFalhaComErroStatusInvalido_QuandoNaoPausada()
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var grupoId = Guid.NewGuid();
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead1", "11911111111", null, null, null, grupoId);
        var criada = await new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork())
            .ExecutarAsync("Campanha", "Msg", grupoId, null);
        var sessaoRepositorio = new FakeSessaoWhatsAppRepository();
        await sessaoRepositorio.AtualizarAsync(new SessaoWhatsApp(StatusSessaoWhatsApp.Conectado));
        var scheduler = new FakeCampanhaJobScheduler();
        var useCase = new RetomarCampanhaUseCase(campanhaRepositorio, sessaoRepositorio, scheduler);

        var resultado = await useCase.ExecutarAsync(criada.Valor);

        Assert.False(resultado.Sucesso);
        Assert.Equal(RetomarCampanhaErro.StatusInvalido, resultado.Erro);
    }
}
