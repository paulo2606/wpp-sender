using WppSender.Domain;
using Xunit;

namespace WppSender.Domain.Tests;

public class EnvioTests
{
    [Fact]
    public void DeveCriarComStatusPendente()
    {
        var envio = new Envio(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(StatusEnvio.Pendente, envio.Status);
    }

    [Fact]
    public void MarcarComoEnviado_DeveAtualizarStatusETimestamp()
    {
        var envio = new Envio(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var agora = new DateTime(2026, 8, 2, 10, 0, 0, DateTimeKind.Utc);

        envio.MarcarComoEnviado(agora, "wamid.123");

        Assert.Equal(StatusEnvio.Enviado, envio.Status);
        Assert.Equal("wamid.123", envio.WhatsAppMensagemId);
        Assert.Equal(agora, envio.EnviadoEm);
        Assert.Equal(agora, envio.AtualizadoEm);
        Assert.Null(envio.Erro);
    }

    [Fact]
    public void MarcarComoEntregue_DeveAtualizarStatus_QuandoVemDeEnviado()
    {
        var envio = new Envio(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var agora = new DateTime(2026, 8, 2, 10, 0, 0, DateTimeKind.Utc);
        envio.MarcarComoEnviado(agora, "wamid.123");

        envio.MarcarComoEntregue(agora.AddSeconds(5));

        Assert.Equal(StatusEnvio.Entregue, envio.Status);
    }

    [Fact]
    public void MarcarComoEntregue_DeveLancar_QuandoNaoVemDeEnviado()
    {
        var envio = new Envio(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => envio.MarcarComoEntregue(DateTime.UtcNow));
    }

    [Fact]
    public void MarcarComoLido_DeveAtualizarStatus_QuandoVemDeEntregue()
    {
        var envio = new Envio(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var agora = DateTime.UtcNow;
        envio.MarcarComoEnviado(agora, "wamid.123");
        envio.MarcarComoEntregue(agora);

        envio.MarcarComoLido(agora.AddSeconds(5));

        Assert.Equal(StatusEnvio.Lido, envio.Status);
    }

    [Fact]
    public void MarcarComoFalhouEntrega_DeveGuardarMotivo_QuandoVemDeEnviado()
    {
        var envio = new Envio(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var agora = DateTime.UtcNow;
        envio.MarcarComoEnviado(agora, "wamid.123");

        envio.MarcarComoFalhouEntrega("Sem confirmação de entrega após timeout", agora.AddMinutes(10));

        Assert.Equal(StatusEnvio.FalhouEntrega, envio.Status);
        Assert.Equal("Sem confirmação de entrega após timeout", envio.Erro);
    }

    [Fact]
    public void MarcarComoFalhou_DeveGuardarMotivo()
    {
        var envio = new Envio(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        envio.MarcarComoFalhou("Número inválido");

        Assert.Equal(StatusEnvio.Falhou, envio.Status);
        Assert.Equal("Número inválido", envio.Erro);
    }

    [Fact]
    public void MarcarComoFalhou_DeveRegistrarAtualizadoEm()
    {
        var envio = new Envio(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        envio.MarcarComoFalhou("Número inválido");

        Assert.NotNull(envio.AtualizadoEm);
    }

    [Fact]
    public void Resetar_DeveVoltarParaPendenteELimparErro()
    {
        var envio = new Envio(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        envio.MarcarComoFalhou("Erro qualquer");

        envio.Resetar();

        Assert.Equal(StatusEnvio.Pendente, envio.Status);
        Assert.Null(envio.Erro);
        Assert.Null(envio.EnviadoEm);
        Assert.Null(envio.AtualizadoEm);
    }
}
