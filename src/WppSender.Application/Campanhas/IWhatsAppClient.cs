using WppSender.Domain;

namespace WppSender.Application.Campanhas;

public interface IWhatsAppClient
{
    Task<ResultadoEnvioMensagem> EnviarMensagemAsync(string telefone, string mensagem);
    Task<string> IniciarSessaoAsync();
    Task<StatusSessaoWhatsApp> ObterStatusSessaoAsync();
}
