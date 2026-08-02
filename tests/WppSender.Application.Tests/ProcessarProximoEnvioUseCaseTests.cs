using WppSender.Application.Campanhas;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using WppSender.Domain;
using Xunit;

namespace WppSender.Application.Tests;

public class ProcessarProximoEnvioUseCaseTests
{
    private class Cenario
    {
        public FakeCampanhaRepository CampanhaRepositorio { get; } = new();
        public FakeEnvioRepository EnvioRepositorio { get; } = new();
        public FakeLeadRepository LeadRepositorio { get; } = new();
        public FakeConfiguracaoEnvioRepository ConfigRepositorio { get; set; } = new();
        public FakeSessaoWhatsAppRepository SessaoRepositorio { get; } = new();
        public FakeWhatsAppClient WhatsAppClient { get; } = new();
        public FakeCampanhaJobScheduler Scheduler { get; } = new();
        public FakeRelogio Relogio { get; } = new();
        public Guid CampanhaId { get; set; }

        public ProcessarProximoEnvioUseCase CriarUseCase() => new(
            CampanhaRepositorio, EnvioRepositorio, LeadRepositorio, ConfigRepositorio,
            SessaoRepositorio, WhatsAppClient, Scheduler, Relogio);
    }

    private static async Task<Cenario> CriarCenarioEmAndamentoAsync(int quantidadeLeads = 1)
    {
        var cenario = new Cenario();
        var grupoId = Guid.NewGuid();
        for (var i = 0; i < quantidadeLeads; i++)
        {
            await new CriarLeadUseCase(cenario.LeadRepositorio).ExecutarAsync($"Lead{i}", $"1191111111{i}", null, null, null, grupoId);
        }

        var criada = await new CriarCampanhaUseCase(cenario.CampanhaRepositorio, cenario.EnvioRepositorio, cenario.LeadRepositorio, new FakeUnitOfWork())
            .ExecutarAsync("Campanha", "Olá {{nome}}", grupoId, null);
        var campanha = await cenario.CampanhaRepositorio.BuscarPorIdAsync(criada.CampanhaId!.Value);
        campanha!.Iniciar();
        await cenario.CampanhaRepositorio.AtualizarAsync(campanha);
        await cenario.SessaoRepositorio.AtualizarAsync(new SessaoWhatsApp(StatusSessaoWhatsApp.Conectado));
        cenario.CampanhaId = campanha.Id;

        return cenario;
    }

    [Fact]
    public async Task DeveEnviarSubstituindoNomeEAgendarProximoPasso_QuandoSucesso()
    {
        var cenario = await CriarCenarioEmAndamentoAsync(quantidadeLeads: 2);
        var useCase = cenario.CriarUseCase();

        await useCase.ExecutarAsync(cenario.CampanhaId);

        Assert.Single(cenario.WhatsAppClient.MensagensEnviadas);
        Assert.Equal("Olá Lead0", cenario.WhatsAppClient.MensagensEnviadas[0].Mensagem);
        var contagens = await cenario.EnvioRepositorio.ContarPorStatusAsync(cenario.CampanhaId);
        Assert.Equal(1, contagens[StatusEnvio.Enviado]);
        Assert.Equal(1, contagens[StatusEnvio.Pendente]);
        Assert.Single(cenario.Scheduler.Agendamentos);
        Assert.Equal(cenario.CampanhaId, cenario.Scheduler.Agendamentos[0].CampanhaId);
    }

    [Fact]
    public async Task DeveMarcarComoFalhou_QuandoEnvioFalha()
    {
        var cenario = await CriarCenarioEmAndamentoAsync();
        cenario.WhatsAppClient.ProximoEnvioDeveFalhar = true;
        cenario.WhatsAppClient.MotivoFalha = "Número inválido";
        var useCase = cenario.CriarUseCase();

        await useCase.ExecutarAsync(cenario.CampanhaId);

        var contagens = await cenario.EnvioRepositorio.ContarPorStatusAsync(cenario.CampanhaId);
        Assert.Equal(1, contagens[StatusEnvio.Falhou]);
        // Mesmo com falha no envio, o motor segue: como não sobrou pendente, conclui em vez de reagendar.
        var campanha = await cenario.CampanhaRepositorio.BuscarPorIdAsync(cenario.CampanhaId);
        Assert.Equal(StatusCampanha.Concluida, campanha!.Status);
    }

    [Fact]
    public async Task NaoDeveFazerNada_QuandoCampanhaNaoEstaEmAndamento()
    {
        var cenario = await CriarCenarioEmAndamentoAsync();
        var campanha = await cenario.CampanhaRepositorio.BuscarPorIdAsync(cenario.CampanhaId);
        campanha!.Pausar();
        await cenario.CampanhaRepositorio.AtualizarAsync(campanha);
        var useCase = cenario.CriarUseCase();

        await useCase.ExecutarAsync(cenario.CampanhaId);

        Assert.Empty(cenario.WhatsAppClient.MensagensEnviadas);
        Assert.Empty(cenario.Scheduler.Agendamentos);
    }

    [Fact]
    public async Task DevePausarCampanha_QuandoSessaoDesconectada()
    {
        var cenario = await CriarCenarioEmAndamentoAsync();
        await cenario.SessaoRepositorio.AtualizarAsync(new SessaoWhatsApp(StatusSessaoWhatsApp.Desconectado));
        var useCase = cenario.CriarUseCase();

        await useCase.ExecutarAsync(cenario.CampanhaId);

        var campanha = await cenario.CampanhaRepositorio.BuscarPorIdAsync(cenario.CampanhaId);
        Assert.Equal(StatusCampanha.Pausada, campanha!.Status);
        Assert.Empty(cenario.WhatsAppClient.MensagensEnviadas);
        Assert.Empty(cenario.Scheduler.Agendamentos);
    }

    [Fact]
    public async Task DevePausarCampanha_QuandoLimiteDiarioAtingido()
    {
        var cenario = await CriarCenarioEmAndamentoAsync();
        cenario.ConfigRepositorio = new FakeConfiguracaoEnvioRepository(limiteDiarioEnvios: 0);
        var useCase = cenario.CriarUseCase();

        await useCase.ExecutarAsync(cenario.CampanhaId);

        var campanha = await cenario.CampanhaRepositorio.BuscarPorIdAsync(cenario.CampanhaId);
        Assert.Equal(StatusCampanha.Pausada, campanha!.Status);
        Assert.Empty(cenario.WhatsAppClient.MensagensEnviadas);
        Assert.Empty(cenario.Scheduler.Agendamentos);
    }

    [Fact]
    public async Task DeveConcluirCampanha_QuandoNaoSobraPendente()
    {
        var cenario = await CriarCenarioEmAndamentoAsync(quantidadeLeads: 1);
        var useCase = cenario.CriarUseCase();
        await useCase.ExecutarAsync(cenario.CampanhaId);
        cenario.Scheduler.Agendamentos.Clear();

        await useCase.ExecutarAsync(cenario.CampanhaId);

        var campanha = await cenario.CampanhaRepositorio.BuscarPorIdAsync(cenario.CampanhaId);
        Assert.Equal(StatusCampanha.Concluida, campanha!.Status);
        Assert.Empty(cenario.Scheduler.Agendamentos);
    }
}
