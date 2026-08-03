using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using WppSender.Application.Campanhas;
using WppSender.Domain;

namespace WppSender.Infrastructure.WhatsApp;

public class HttpWhatsAppClient : IWhatsAppClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpWhatsAppClient> _logger;

    public HttpWhatsAppClient(HttpClient httpClient, ILogger<HttpWhatsAppClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
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
        try
        {
            var resposta = await _httpClient.PostAsync("sessao/iniciar", null);

            if (!resposta.IsSuccessStatusCode)
            {
                _logger.LogError("Falha ao iniciar sessao do WhatsApp: status {StatusCode}", resposta.StatusCode);
                return string.Empty;
            }

            var corpo = await resposta.Content.ReadFromJsonAsync<IniciarSessaoRespostaDto>();
            return corpo?.QrCodeBase64 ?? string.Empty;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Falha ao comunicar com o microservico do WhatsApp ao iniciar sessao");
            return string.Empty;
        }
    }

    public async Task<StatusSessaoWhatsApp> ObterStatusSessaoAsync()
    {
        try
        {
            var resposta = await _httpClient.GetAsync("sessao/status");

            if (!resposta.IsSuccessStatusCode)
            {
                _logger.LogError("Falha ao obter status da sessao do WhatsApp: status {StatusCode}", resposta.StatusCode);
                return StatusSessaoWhatsApp.Desconectado;
            }

            var corpo = await resposta.Content.ReadFromJsonAsync<StatusSessaoRespostaDto>();
            return corpo?.Status switch
            {
                "conectado" => StatusSessaoWhatsApp.Conectado,
                "aguardando_qr" => StatusSessaoWhatsApp.AguardandoQr,
                _ => StatusSessaoWhatsApp.Desconectado,
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Falha ao comunicar com o microservico do WhatsApp ao obter status da sessao");
            return StatusSessaoWhatsApp.Desconectado;
        }
    }

    private record IniciarSessaoRespostaDto(string QrCodeBase64);
    private record StatusSessaoRespostaDto(string Status);
}
