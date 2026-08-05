using WppSender.Application.Campanhas;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using WppSender.Domain;
using Xunit;

namespace WppSender.Application.Tests;

public class ReenviarFalhasUseCaseTests
{
    [Fact]
    public async Task DeveResetarFalhosParaPendenteEReabrirCampanhaConcluida()
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var grupoId = Guid.NewGuid();
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead1", "11911111111", null, null, null, grupoId);
        var criada = await new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork())
            .ExecutarAsync("Campanha", "Msg", grupoId, null);
        var campanha = await campanhaRepositorio.BuscarPorIdAsync(criada.CampanhaId!.Value);
        var envio = envioRepositorio.Todos.First(e => e.CampanhaId == campanha!.Id);
        envio.MarcarComoFalhou("Erro");
        await envioRepositorio.AtualizarAsync(envio);
        campanha!.Iniciar();
        campanha.Concluir();
        await campanhaRepositorio.AtualizarAsync(campanha);
        var scheduler = new FakeCampanhaJobScheduler();
        var useCase = new ReenviarFalhasUseCase(campanhaRepositorio, envioRepositorio, scheduler);

        await useCase.ExecutarAsync(campanha.Id);

        var contagens = await envioRepositorio.ContarPorStatusAsync(campanha.Id);
        Assert.Equal(1, contagens[StatusEnvio.Pendente]);
        Assert.False(contagens.ContainsKey(StatusEnvio.Falhou));
        var recarregada = await campanhaRepositorio.BuscarPorIdAsync(campanha.Id);
        Assert.Equal(StatusCampanha.EmAndamento, recarregada!.Status);
        Assert.Single(scheduler.Agendamentos);
    }

    [Fact]
    public async Task DeveReagendar_QuandoCampanhaJaEstaEmAndamento()
    {
        // Campanha EmAndamento não garante que a cadeia de disparo ainda esteja viva: ela
        // pode ter esgotado os pendentes e parado de reagendar sozinha (aguardando só
        // confirmação de entrega). Por isso sempre reagenda aqui — é seguro mesmo se a
        // cadeia original ainda estiver ativa, pois o lock distribuído por campanha em
        // CampanhaSendJob serializa as execuções concorrentes.
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var grupoId = Guid.NewGuid();
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead1", "11911111111", null, null, null, grupoId);
        var criada = await new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork())
            .ExecutarAsync("Campanha", "Msg", grupoId, null);
        var campanha = await campanhaRepositorio.BuscarPorIdAsync(criada.CampanhaId!.Value);
        var envio = envioRepositorio.Todos.First(e => e.CampanhaId == campanha!.Id);
        envio.MarcarComoFalhou("Erro");
        await envioRepositorio.AtualizarAsync(envio);
        campanha!.Iniciar();
        await campanhaRepositorio.AtualizarAsync(campanha);
        var scheduler = new FakeCampanhaJobScheduler();
        var useCase = new ReenviarFalhasUseCase(campanhaRepositorio, envioRepositorio, scheduler);

        await useCase.ExecutarAsync(campanha.Id);

        var contagens = await envioRepositorio.ContarPorStatusAsync(campanha.Id);
        Assert.Equal(1, contagens[StatusEnvio.Pendente]);
        var recarregada = await campanhaRepositorio.BuscarPorIdAsync(campanha.Id);
        Assert.Equal(StatusCampanha.EmAndamento, recarregada!.Status);
        Assert.Single(scheduler.Agendamentos);
    }

    [Fact]
    public async Task NaoDeveReagendar_QuandoNaoHaFalhos()
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var grupoId = Guid.NewGuid();
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead1", "11911111111", null, null, null, grupoId);
        var criada = await new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork())
            .ExecutarAsync("Campanha", "Msg", grupoId, null);
        var scheduler = new FakeCampanhaJobScheduler();
        var useCase = new ReenviarFalhasUseCase(campanhaRepositorio, envioRepositorio, scheduler);

        await useCase.ExecutarAsync(criada.CampanhaId!.Value);

        Assert.Empty(scheduler.Agendamentos);
    }
}
