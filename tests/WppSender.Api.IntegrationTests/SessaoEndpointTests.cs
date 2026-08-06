using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using WppSender.Application.Campanhas;
using WppSender.Domain;
using Xunit;

namespace WppSender.Api.IntegrationTests;

public class SessaoEndpointTests : IAsyncLifetime
{
    private WppSenderApiFactory _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _factory = new WppSenderApiFactory();
        await _factory.InitializeAsync();
        _client = _factory.CreateClient();

        var registro = await _client.PostAsJsonAsync("/api/auth/registrar", new { email = "admin@teste.com", senha = "senhaAdmin123" });
        registro.EnsureSuccessStatusCode();
        var login = await _client.PostAsJsonAsync("/api/auth/login", new { email = "admin@teste.com", senha = "senhaAdmin123" });
        login.EnsureSuccessStatusCode();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task DeveIniciarSessaoERetornarQrCode()
    {
        var resposta = await _client.PostAsync("/api/sessao/iniciar", null);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var corpo = await resposta.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.False(string.IsNullOrEmpty(corpo!["qrCodeBase64"]));
    }

    [Fact]
    public async Task DeveRetornarStatusDesconectadoPorPadrao()
    {
        var resposta = await _client.GetAsync("/api/sessao/status");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("Desconectado", corpo);
    }

    [Fact]
    public async Task FluxoCompleto_DeveEnviarParaTodosOsLeadsEConcluirCampanha_QuandoSessaoConectada()
    {

        var lead1 = await _client.PostAsJsonAsync("/api/leads", new { nome = "Lead1", telefone = "11911111111", instagram = (string?)null, endereco = (object?)null, origem = (string?)null, grupoId = (Guid?)null });
        var lead1Corpo = await lead1.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var lead2 = await _client.PostAsJsonAsync("/api/leads", new { nome = "Lead2", telefone = "11922222222", instagram = (string?)null, endereco = (object?)null, origem = (string?)null, grupoId = (Guid?)null });
        var lead2Corpo = await lead2.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var grupo = await _client.PostAsJsonAsync("/api/grupos", new { nome = "Grupo E2E", descricao = (string?)null, leadIds = new[] { lead1Corpo!["id"], lead2Corpo!["id"] } });
        var grupoCorpo = await grupo.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var campanha = await _client.PostAsJsonAsync("/api/campanhas", new { nome = "Campanha E2E", mensagem = "Olá {{nome}}", grupoId = grupoCorpo!["id"], agendadoPara = (DateTime?)null });
        var campanhaCorpo = await campanha.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        using (var escopo = _factory.Services.CreateScope())
        {
            var sessaoRepositorio = escopo.ServiceProvider.GetRequiredService<ISessaoWhatsAppRepository>();
            var sessao = await sessaoRepositorio.ObterAsync();
            sessao.MarcarConectado();
            await sessaoRepositorio.AtualizarAsync(sessao);
        }

        var iniciar = await _client.PostAsync($"/api/campanhas/{campanhaCorpo!["id"]}/iniciar", null);
        Assert.Equal(HttpStatusCode.OK, iniciar.StatusCode);

        using (var escopo = _factory.Services.CreateScope())
        {
            var processarUseCase = escopo.ServiceProvider.GetRequiredService<ProcessarProximoEnvioUseCase>();
            await processarUseCase.ExecutarAsync(campanhaCorpo["id"]);
            await processarUseCase.ExecutarAsync(campanhaCorpo["id"]);
        }

        using (var escopo = _factory.Services.CreateScope())
        {

            var atualizarStatusUseCase = escopo.ServiceProvider.GetRequiredService<AtualizarStatusEntregaUseCase>();
            await atualizarStatusUseCase.ExecutarAsync();
        }

        var detalhe = await _client.GetAsync($"/api/campanhas/{campanhaCorpo["id"]}");
        var detalheTexto = await detalhe.Content.ReadAsStringAsync();
        Assert.Contains("\"entregue\":2", detalheTexto.ToLowerInvariant());
        Assert.Contains("\"concluida\"", detalheTexto.ToLowerInvariant());
    }
}
