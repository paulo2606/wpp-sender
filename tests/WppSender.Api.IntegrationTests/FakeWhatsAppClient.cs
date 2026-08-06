using WppSender.Application.Campanhas;
using WppSender.Domain;
using System.Linq;

namespace WppSender.Api.IntegrationTests;

public class FakeWhatsAppClient : IWhatsAppClient
{
    public Task<ResultadoEnvioMensagem> EnviarMensagemAsync(string telefone, string mensagem) =>
        Task.FromResult(new ResultadoEnvioMensagem(true, null, Guid.NewGuid().ToString()));

    public Task<string> IniciarSessaoAsync() => Task.FromResult("qr-code-base64-fake");

    public Task<StatusSessaoWhatsApp> ObterStatusSessaoAsync() => Task.FromResult(StatusSessaoWhatsApp.Desconectado);

    public Task<IReadOnlyDictionary<string, StatusEntregaMensagem>> ObterStatusMensagensAsync(IReadOnlyCollection<string> mensagemIds) =>
        Task.FromResult<IReadOnlyDictionary<string, StatusEntregaMensagem>>(
            mensagemIds.ToDictionary(id => id, _ => StatusEntregaMensagem.Entregue));
}
