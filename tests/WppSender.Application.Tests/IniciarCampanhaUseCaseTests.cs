using WppSender.Application.Campanhas;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using WppSender.Domain;
using Xunit;

namespace WppSender.Application.Tests;

public class IniciarCampanhaUseCaseTests
{
    private static async Task<(FakeCampanhaRepository CampanhaRepositorio, FakeSessaoWhatsAppRepository SessaoRepositorio, FakeCampanhaJobScheduler Scheduler, Guid CampanhaId)> CriarCenarioAsync()
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var grupoId = Guid.NewGuid();
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead1", "11911111111", null, null, null, grupoId);
        var criada = await new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork())
            .ExecutarAsync("Campanha", "Msg", grupoId, null);
        var sessaoRepositorio = new FakeSessaoWhatsAppRepository();
        var scheduler = new FakeCampanhaJobScheduler();

        return (campanhaRepositorio, sessaoRepositorio, scheduler, criada.CampanhaId!.Value);
    }

    [Fact]
    public async Task DeveIniciarEAgendarPrimeiroPasso_QuandoSessaoConectada()
    {
        var (campanhaRepositorio, sessaoRepositorio, scheduler, campanhaId) = await CriarCenarioAsync();
        await sessaoRepositorio.AtualizarAsync(new SessaoWhatsApp(StatusSessaoWhatsApp.Conectado));
        var useCase = new IniciarCampanhaUseCase(campanhaRepositorio, sessaoRepositorio, scheduler);

        var resultado = await useCase.ExecutarAsync(campanhaId);

        Assert.True(resultado.Sucesso);
        var campanha = await campanhaRepositorio.BuscarPorIdAsync(campanhaId);
        Assert.Equal(StatusCampanha.EmAndamento, campanha!.Status);
        Assert.Single(scheduler.Agendamentos);
        Assert.Equal(campanhaId, scheduler.Agendamentos[0].CampanhaId);
        Assert.Equal(TimeSpan.Zero, scheduler.Agendamentos[0].Atraso);
    }

    [Fact]
    public async Task DeveRetornarFalhaComErroSessaoDesconectada_QuandoSessaoNaoConectada()
    {
        var (campanhaRepositorio, sessaoRepositorio, scheduler, campanhaId) = await CriarCenarioAsync();
        var useCase = new IniciarCampanhaUseCase(campanhaRepositorio, sessaoRepositorio, scheduler);

        var resultado = await useCase.ExecutarAsync(campanhaId);

        Assert.False(resultado.Sucesso);
        Assert.Equal(IniciarCampanhaErro.SessaoDesconectada, resultado.Erro);
        var campanha = await campanhaRepositorio.BuscarPorIdAsync(campanhaId);
        Assert.Equal(StatusCampanha.Rascunho, campanha!.Status);
        Assert.Empty(scheduler.Agendamentos);
    }

    [Fact]
    public async Task DeveRetornarFalhaComErroNaoEncontrada_QuandoCampanhaNaoExiste()
    {
        var sessaoRepositorio = new FakeSessaoWhatsAppRepository();
        var scheduler = new FakeCampanhaJobScheduler();
        var useCase = new IniciarCampanhaUseCase(new FakeCampanhaRepository(), sessaoRepositorio, scheduler);

        var resultado = await useCase.ExecutarAsync(Guid.NewGuid());

        Assert.False(resultado.Sucesso);
        Assert.Equal(IniciarCampanhaErro.NaoEncontrada, resultado.Erro);
    }
}
