using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Xunit;

namespace WppSender.Api.IntegrationTests;

public class DashboardEndpointTests : IAsyncLifetime
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
        var corpo = await login.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", corpo!["token"]);
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }

    private async Task<Guid> CriarLeadAsync(string nome, string telefone, Guid? grupoId = null)
    {
        var resposta = await _client.PostAsJsonAsync("/api/leads", new
        {
            nome,
            telefone,
            instagram = (string?)null,
            endereco = (object?)null,
            origem = (string?)null,
            grupoId,
        });
        var corpoResposta = await resposta.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        return corpoResposta!["id"];
    }

    [Fact]
    public async Task DeveRetornarContagemDeCampanhasPorStatus()
    {
        var lead = await CriarLeadAsync("Lead Dashboard", "11955555501");
        var grupo = await _client.PostAsJsonAsync("/api/grupos", new { nome = "Grupo Dashboard", descricao = (string?)null, leadIds = new[] { lead } });
        var grupoCorpo = await grupo.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        await _client.PostAsJsonAsync("/api/campanhas", new { nome = "Campanha Dashboard", mensagem = "Msg", grupoId = grupoCorpo!["id"], agendadoPara = (DateTime?)null });

        var resposta = await _client.GetAsync("/api/dashboard/campanhas-por-status");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var texto = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("\"rascunho\":1", texto.ToLowerInvariant());
    }

    [Fact]
    public async Task DeveRetornarLeadsRecentesComDiasPadrao()
    {
        await CriarLeadAsync("Lead Recente", "11955555502");

        var resposta = await _client.GetAsync("/api/dashboard/leads-recentes");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var texto = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("\"dias\":7", texto.ToLowerInvariant());
    }

    [Fact]
    public async Task DeveRetornarLeadsPorGrupo()
    {
        var lead = await CriarLeadAsync("Lead Grupo", "11955555503");
        await _client.PostAsJsonAsync("/api/grupos", new { nome = "Grupo Contagem", descricao = (string?)null, leadIds = new[] { lead } });

        var resposta = await _client.GetAsync("/api/dashboard/leads-por-grupo");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var texto = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("Grupo Contagem", texto);
        Assert.Contains("\"quantidade\":1", texto.ToLowerInvariant());
    }

    [Fact]
    public async Task DeveRetornarNotFound_QuandoNaoHaCampanhaAgendada()
    {
        var resposta = await _client.GetAsync("/api/dashboard/proxima-campanha-agendada");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task DeveRetornarTaxaDeEnvioZerada_QuandoNaoHaEnvios()
    {
        var resposta = await _client.GetAsync("/api/dashboard/taxa-envio");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var texto = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("\"enviado\":0", texto.ToLowerInvariant());
    }
}
