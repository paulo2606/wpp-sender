using WppSender.Domain;

namespace WppSender.Application.Campanhas;

public class ObterStatusSessaoUseCase
{
    private readonly IWhatsAppClient _whatsAppClient;
    private readonly ISessaoWhatsAppRepository _sessaoRepositorio;

    public ObterStatusSessaoUseCase(IWhatsAppClient whatsAppClient, ISessaoWhatsAppRepository sessaoRepositorio)
    {
        _whatsAppClient = whatsAppClient;
        _sessaoRepositorio = sessaoRepositorio;
    }

    public async Task<StatusSessaoWhatsApp> ExecutarAsync()
    {
        var statusAtual = await _whatsAppClient.ObterStatusSessaoAsync();

        var sessao = await _sessaoRepositorio.ObterAsync();
        switch (statusAtual)
        {
            case StatusSessaoWhatsApp.Conectado:
                sessao.MarcarConectado();
                break;
            case StatusSessaoWhatsApp.AguardandoQr:
                sessao.MarcarAguardandoQr();
                break;
            default:
                sessao.MarcarDesconectado();
                break;
        }

        await _sessaoRepositorio.AtualizarAsync(sessao);

        return statusAtual;
    }
}
