using WppSender.Application.Campanhas;
using WppSender.Application.Tests.Fakes;
using WppSender.Domain;
using Xunit;

namespace WppSender.Application.Tests;

public class ObterStatusSessaoUseCaseTests
{
    [Fact]
    public async Task DeveSincronizarComOWhatsAppClientEPersistir()
    {
        var whatsAppClient = new FakeWhatsAppClient { StatusSessao = StatusSessaoWhatsApp.Conectado };
        var sessaoRepositorio = new FakeSessaoWhatsAppRepository();
        var useCase = new ObterStatusSessaoUseCase(whatsAppClient, sessaoRepositorio);

        var status = await useCase.ExecutarAsync();

        Assert.Equal(StatusSessaoWhatsApp.Conectado, status);
        var sessao = await sessaoRepositorio.ObterAsync();
        Assert.Equal(StatusSessaoWhatsApp.Conectado, sessao.Status);
    }

    [Fact]
    public async Task DeveSincronizarComoDesconectado_QuandoWhatsAppClientRetornaDesconectado()
    {
        var whatsAppClient = new FakeWhatsAppClient { StatusSessao = StatusSessaoWhatsApp.Desconectado };
        var sessaoRepositorio = new FakeSessaoWhatsAppRepository();
        await sessaoRepositorio.AtualizarAsync(new SessaoWhatsApp(StatusSessaoWhatsApp.Conectado));
        var useCase = new ObterStatusSessaoUseCase(whatsAppClient, sessaoRepositorio);

        var status = await useCase.ExecutarAsync();

        Assert.Equal(StatusSessaoWhatsApp.Desconectado, status);
    }
}
