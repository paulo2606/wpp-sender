using WppSender.Domain;
using Xunit;

namespace WppSender.Domain.Tests;

public class SessaoWhatsAppTests
{
    [Fact]
    public void MarcarAguardandoQr_DeveAtualizarStatus()
    {
        var sessao = new SessaoWhatsApp(StatusSessaoWhatsApp.Desconectado);

        sessao.MarcarAguardandoQr();

        Assert.Equal(StatusSessaoWhatsApp.AguardandoQr, sessao.Status);
    }

    [Fact]
    public void MarcarConectado_DeveAtualizarStatus()
    {
        var sessao = new SessaoWhatsApp(StatusSessaoWhatsApp.AguardandoQr);

        sessao.MarcarConectado();

        Assert.Equal(StatusSessaoWhatsApp.Conectado, sessao.Status);
    }

    [Fact]
    public void MarcarDesconectado_DeveAtualizarStatus()
    {
        var sessao = new SessaoWhatsApp(StatusSessaoWhatsApp.Conectado);

        sessao.MarcarDesconectado();

        Assert.Equal(StatusSessaoWhatsApp.Desconectado, sessao.Status);
    }
}
