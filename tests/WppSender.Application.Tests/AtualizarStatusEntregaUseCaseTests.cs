using WppSender.Application.Campanhas;
using WppSender.Application.Tests.Fakes;
using WppSender.Domain;
using Xunit;

namespace WppSender.Application.Tests;

public class AtualizarStatusEntregaUseCaseTests
{
    private class Cenario
    {
        public FakeCampanhaRepository CampanhaRepositorio { get; } = new();
        public FakeEnvioRepository EnvioRepositorio { get; } = new();
        public FakeWhatsAppClient WhatsAppClient { get; } = new();
        public FakeRelogio Relogio { get; } = new();

        public AtualizarStatusEntregaUseCase CriarUseCase(int timeoutConfirmacaoMinutos = 10) => new(
            EnvioRepositorio, CampanhaRepositorio, WhatsAppClient, Relogio, timeoutConfirmacaoMinutos);

        public async Task<(Campanha Campanha, Envio Envio)> CriarEnvioEmAndamentoAsync(string mensagemId, DateTime enviadoEm)
        {
            var campanha = new Campanha(Guid.NewGuid(), "Campanha", "Msg", Guid.NewGuid(), null);
            campanha.Iniciar();
            await CampanhaRepositorio.AdicionarAsync(campanha);

            var envio = new Envio(Guid.NewGuid(), campanha.Id, Guid.NewGuid());
            envio.MarcarComoEnviado(enviadoEm, mensagemId);
            await EnvioRepositorio.AdicionarVariosAsync(new[] { envio });

            return (campanha, envio);
        }
    }

    [Fact]
    public async Task DeveMarcarComoEntregue_QuandoAckReportaEntregue()
    {
        var cenario = new Cenario();
        var (_, envio) = await cenario.CriarEnvioEmAndamentoAsync("wamid.1", cenario.Relogio.Agora);
        cenario.WhatsAppClient.StatusPorMensagemId["wamid.1"] = StatusEntregaMensagem.Entregue;

        await cenario.CriarUseCase().ExecutarAsync();

        Assert.Equal(StatusEnvio.Entregue, cenario.EnvioRepositorio.Todos.Single(e => e.Id == envio.Id).Status);
    }

    [Fact]
    public async Task DeveMarcarComoLido_QuandoAckReportaLido()
    {
        var cenario = new Cenario();
        var (_, envio) = await cenario.CriarEnvioEmAndamentoAsync("wamid.1", cenario.Relogio.Agora);
        cenario.WhatsAppClient.StatusPorMensagemId["wamid.1"] = StatusEntregaMensagem.Lido;

        await cenario.CriarUseCase().ExecutarAsync();

        Assert.Equal(StatusEnvio.Lido, cenario.EnvioRepositorio.Todos.Single(e => e.Id == envio.Id).Status);
    }

    [Fact]
    public async Task DeveMarcarComoFalhouEntrega_QuandoAckReportaErro()
    {
        var cenario = new Cenario();
        var (_, envio) = await cenario.CriarEnvioEmAndamentoAsync("wamid.1", cenario.Relogio.Agora);
        cenario.WhatsAppClient.StatusPorMensagemId["wamid.1"] = StatusEntregaMensagem.Erro;

        await cenario.CriarUseCase().ExecutarAsync();

        var atualizado = cenario.EnvioRepositorio.Todos.Single(e => e.Id == envio.Id);
        Assert.Equal(StatusEnvio.FalhouEntrega, atualizado.Status);
        Assert.NotNull(atualizado.Erro);
    }

    [Fact]
    public async Task DeveMarcarComoFalhouEntregaPorTimeout_QuandoSemAckAposLimite()
    {
        var cenario = new Cenario();
        var enviadoEm = cenario.Relogio.Agora.AddMinutes(-15);
        var (_, envio) = await cenario.CriarEnvioEmAndamentoAsync("wamid.1", enviadoEm);
        // Sem entrada em StatusPorMensagemId: simula ACK que nunca chegou.

        await cenario.CriarUseCase(timeoutConfirmacaoMinutos: 10).ExecutarAsync();

        var atualizado = cenario.EnvioRepositorio.Todos.Single(e => e.Id == envio.Id);
        Assert.Equal(StatusEnvio.FalhouEntrega, atualizado.Status);
        Assert.Equal("Sem confirmação de entrega após timeout", atualizado.Erro);
    }

    [Fact]
    public async Task NaoDeveAlterarStatus_QuandoDentroDoTimeoutESemAck()
    {
        var cenario = new Cenario();
        var enviadoEm = cenario.Relogio.Agora.AddMinutes(-2);
        var (_, envio) = await cenario.CriarEnvioEmAndamentoAsync("wamid.1", enviadoEm);

        await cenario.CriarUseCase(timeoutConfirmacaoMinutos: 10).ExecutarAsync();

        Assert.Equal(StatusEnvio.Enviado, cenario.EnvioRepositorio.Todos.Single(e => e.Id == envio.Id).Status);
    }

    [Fact]
    public async Task DeveConcluirCampanha_QuandoTodosOsEnviosChegamAUmStatusFinal()
    {
        var cenario = new Cenario();
        var (campanha, _) = await cenario.CriarEnvioEmAndamentoAsync("wamid.1", cenario.Relogio.Agora);
        cenario.WhatsAppClient.StatusPorMensagemId["wamid.1"] = StatusEntregaMensagem.Entregue;

        await cenario.CriarUseCase().ExecutarAsync();

        var atualizada = await cenario.CampanhaRepositorio.BuscarPorIdAsync(campanha.Id);
        Assert.Equal(StatusCampanha.Concluida, atualizada!.Status);
    }

    [Fact]
    public async Task NaoDeveConcluirCampanha_QuandoAindaHaEnvioAguardandoConfirmacao()
    {
        var cenario = new Cenario();
        var (campanha, _) = await cenario.CriarEnvioEmAndamentoAsync("wamid.1", cenario.Relogio.Agora);
        // Segundo envio da mesma campanha ainda sem ACK e dentro do timeout.
        var segundoEnvio = new Envio(Guid.NewGuid(), campanha.Id, Guid.NewGuid());
        segundoEnvio.MarcarComoEnviado(cenario.Relogio.Agora, "wamid.2");
        await cenario.EnvioRepositorio.AdicionarVariosAsync(new[] { segundoEnvio });
        cenario.WhatsAppClient.StatusPorMensagemId["wamid.1"] = StatusEntregaMensagem.Entregue;

        await cenario.CriarUseCase().ExecutarAsync();

        var atualizada = await cenario.CampanhaRepositorio.BuscarPorIdAsync(campanha.Id);
        Assert.Equal(StatusCampanha.EmAndamento, atualizada!.Status);
    }

    [Fact]
    public async Task NaoDeveTocarEnvio_QuandoCampanhaJaFoiCancelada()
    {
        var cenario = new Cenario();
        var (campanha, envio) = await cenario.CriarEnvioEmAndamentoAsync("wamid.1", cenario.Relogio.Agora);
        campanha.Pausar();
        campanha.Cancelar();
        cenario.EnvioRepositorio.StatusCampanhas[campanha.Id] = StatusCampanha.Cancelada;
        cenario.WhatsAppClient.StatusPorMensagemId["wamid.1"] = StatusEntregaMensagem.Entregue;

        await cenario.CriarUseCase().ExecutarAsync();

        Assert.Equal(StatusEnvio.Enviado, cenario.EnvioRepositorio.Todos.Single(e => e.Id == envio.Id).Status);
    }
}
