using WppSender.Application.Campanhas;
using WppSender.Domain;
using System.Linq;

namespace WppSender.Api.IntegrationTests;

/// <summary>
/// Substitui o HttpWhatsAppClient real (que dependeria de um serviço WhatsApp/Baileys
/// de verdade em http://localhost:3000) durante os testes de integração. Sempre reporta
/// sucesso no envio e status desconectado por padrão — os testes que precisam de uma
/// sessão conectada manipulam o ISessaoWhatsAppRepository diretamente via DI.
/// </summary>
public class FakeWhatsAppClient : IWhatsAppClient
{
    public Task<ResultadoEnvioMensagem> EnviarMensagemAsync(string telefone, string mensagem) =>
        Task.FromResult(new ResultadoEnvioMensagem(true, null, Guid.NewGuid().ToString()));

    public Task<string> IniciarSessaoAsync() => Task.FromResult("qr-code-base64-fake");

    public Task<StatusSessaoWhatsApp> ObterStatusSessaoAsync() => Task.FromResult(StatusSessaoWhatsApp.Desconectado);

    // Assim como o envio, sempre reporta "entregue" pra quem quer que pergunte —
    // testes que precisam simular timeout/erro de entrega usam um IWhatsAppClient próprio.
    public Task<IReadOnlyDictionary<string, StatusEntregaMensagem>> ObterStatusMensagensAsync(IReadOnlyCollection<string> mensagemIds) =>
        Task.FromResult<IReadOnlyDictionary<string, StatusEntregaMensagem>>(
            mensagemIds.ToDictionary(id => id, _ => StatusEntregaMensagem.Entregue));
}
