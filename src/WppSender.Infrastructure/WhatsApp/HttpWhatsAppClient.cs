using System.Net.Http.Json;
using System.Web;
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

        var corpo = await resposta.Content.ReadFromJsonAsync<EnviarMensagemRespostaDto>();
        return new ResultadoEnvioMensagem(true, null, corpo?.MensagemId);
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

    public async Task<IReadOnlyDictionary<string, StatusEntregaMensagem>> ObterStatusMensagensAsync(IReadOnlyCollection<string> mensagemIds)
    {
        if (mensagemIds.Count == 0)
        {
            return new Dictionary<string, StatusEntregaMensagem>();
        }

        try
        {
            var query = HttpUtility.UrlEncode(string.Join(',', mensagemIds));
            var resposta = await _httpClient.GetAsync($"mensagens/status?ids={query}");

            if (!resposta.IsSuccessStatusCode)
            {
                _logger.LogError("Falha ao obter status de mensagens do WhatsApp: status {StatusCode}", resposta.StatusCode);
                return new Dictionary<string, StatusEntregaMensagem>();
            }

            var corpo = await resposta.Content.ReadFromJsonAsync<Dictionary<string, string>>() ?? new();
            return corpo.ToDictionary(par => par.Key, par => ParaStatusEntregaMensagem(par.Value));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogError(ex, "Falha ao comunicar com o microservico do WhatsApp ao obter status de mensagens");
            return new Dictionary<string, StatusEntregaMensagem>();
        }
    }

    private static StatusEntregaMensagem ParaStatusEntregaMensagem(string valor) => valor switch
    {
        "entregue" => StatusEntregaMensagem.Entregue,
        "lido" => StatusEntregaMensagem.Lido,
        "erro" => StatusEntregaMensagem.Erro,
        _ => StatusEntregaMensagem.Pendente,
    };

    private record IniciarSessaoRespostaDto(string QrCodeBase64);
    private record StatusSessaoRespostaDto(string Status);
    private record EnviarMensagemRespostaDto(string? MensagemId);
}
