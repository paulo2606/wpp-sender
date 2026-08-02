using WppSender.Domain;

namespace WppSender.Application.Campanhas;

public class IniciarSessaoWhatsAppUseCase
{
    private readonly IWhatsAppClient _whatsAppClient;
    private readonly ISessaoWhatsAppRepository _sessaoRepositorio;

    public IniciarSessaoWhatsAppUseCase(IWhatsAppClient whatsAppClient, ISessaoWhatsAppRepository sessaoRepositorio)
    {
        _whatsAppClient = whatsAppClient;
        _sessaoRepositorio = sessaoRepositorio;
    }

    public async Task<string> ExecutarAsync()
    {
        var qrCode = await _whatsAppClient.IniciarSessaoAsync();

        var sessao = await _sessaoRepositorio.ObterAsync();
        sessao.MarcarAguardandoQr();
        await _sessaoRepositorio.AtualizarAsync(sessao);

        return qrCode;
    }
}
