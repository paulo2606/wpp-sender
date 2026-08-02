namespace WppSender.Domain;

public interface ISessaoWhatsAppRepository
{
    Task<SessaoWhatsApp> ObterAsync();
    Task AtualizarAsync(SessaoWhatsApp sessao);
}
