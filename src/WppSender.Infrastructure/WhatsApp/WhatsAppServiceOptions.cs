namespace WppSender.Infrastructure.WhatsApp;

public class WhatsAppServiceOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutConfirmacaoMinutos { get; set; } = 10;
}
