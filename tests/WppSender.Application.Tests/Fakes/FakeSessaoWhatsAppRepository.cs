using WppSender.Domain;

namespace WppSender.Application.Tests.Fakes;

public class FakeSessaoWhatsAppRepository : ISessaoWhatsAppRepository
{
    private SessaoWhatsApp _sessao = new(StatusSessaoWhatsApp.Desconectado);

    public Task<SessaoWhatsApp> ObterAsync() => Task.FromResult(_sessao);

    public Task AtualizarAsync(SessaoWhatsApp sessao)
    {
        _sessao = sessao;
        return Task.CompletedTask;
    }
}
