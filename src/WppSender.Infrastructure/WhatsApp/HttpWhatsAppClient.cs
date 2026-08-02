using System.Net.Http.Json;
using WppSender.Application.Campanhas;
using WppSender.Domain;

namespace WppSender.Infrastructure.WhatsApp;

public class HttpWhatsAppClient : IWhatsAppClient
{
    private readonly HttpClient _httpClient;

    public HttpWhatsAppClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ResultadoEnvioMensagem> EnviarMensagemAsync(string telefone, string mensagem)
    {
        var resposta = await _httpClient.PostAsJsonAsync("mensagens/enviar", new { telefone, mensagem });

        if (!resposta.IsSuccessStatusCode)
        {
            var corpoErro = await resposta.Content.ReadAsStringAsync();
            return new ResultadoEnvioMensagem(false, corpoErro);
        }

        return new ResultadoEnvioMensagem(true, null);
    }

    public async Task<string> IniciarSessaoAsync()
    {
        var resposta = await _httpClient.PostAsync("sessao/iniciar", null);
        resposta.EnsureSuccessStatusCode();

        var corpo = await resposta.Content.ReadFromJsonAsync<IniciarSessaoRespostaDto>();
        return corpo?.QrCodeBase64 ?? string.Empty;
    }

    public async Task<StatusSessaoWhatsApp> ObterStatusSessaoAsync()
    {
        var resposta = await _httpClient.GetAsync("sessao/status");
        resposta.EnsureSuccessStatusCode();

        var corpo = await resposta.Content.ReadFromJsonAsync<StatusSessaoRespostaDto>();
        return corpo?.Status switch
        {
            "conectado" => StatusSessaoWhatsApp.Conectado,
            "aguardando_qr" => StatusSessaoWhatsApp.AguardandoQr,
            _ => StatusSessaoWhatsApp.Desconectado,
        };
    }

    private record IniciarSessaoRespostaDto(string QrCodeBase64);
    private record StatusSessaoRespostaDto(string Status);
}
