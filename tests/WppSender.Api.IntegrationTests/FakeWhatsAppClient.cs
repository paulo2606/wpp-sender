using WppSender.Application.Campanhas;
using WppSender.Domain;

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
        Task.FromResult(new ResultadoEnvioMensagem(true, null));

    public Task<string> IniciarSessaoAsync() => Task.FromResult("qr-code-base64-fake");

    public Task<StatusSessaoWhatsApp> ObterStatusSessaoAsync() => Task.FromResult(StatusSessaoWhatsApp.Desconectado);
}
