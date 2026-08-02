using WppSender.Application.Campanhas;
using WppSender.Application.Tests.Fakes;
using WppSender.Domain;
using Xunit;

namespace WppSender.Application.Tests;

public class IniciarSessaoWhatsAppUseCaseTests
{
    [Fact]
    public async Task DeveRetornarQrCodeEMarcarAguardandoQr()
    {
        var whatsAppClient = new FakeWhatsAppClient();
        var sessaoRepositorio = new FakeSessaoWhatsAppRepository();
        var useCase = new IniciarSessaoWhatsAppUseCase(whatsAppClient, sessaoRepositorio);

        var qrCode = await useCase.ExecutarAsync();

        Assert.Equal("qr-code-base64-fake", qrCode);
        var sessao = await sessaoRepositorio.ObterAsync();
        Assert.Equal(StatusSessaoWhatsApp.AguardandoQr, sessao.Status);
    }
}
