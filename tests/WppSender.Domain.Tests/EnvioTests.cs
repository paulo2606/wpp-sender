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

        envio.MarcarComoEnviado(agora);

        Assert.Equal(StatusEnvio.Enviado, envio.Status);
        Assert.Equal(agora, envio.EnviadoEm);
        Assert.Null(envio.Erro);
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
    public void Resetar_DeveVoltarParaPendenteELimparErro()
    {
        var envio = new Envio(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        envio.MarcarComoFalhou("Erro qualquer");

        envio.Resetar();

        Assert.Equal(StatusEnvio.Pendente, envio.Status);
        Assert.Null(envio.Erro);
        Assert.Null(envio.EnviadoEm);
    }
}
