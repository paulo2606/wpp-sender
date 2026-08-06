using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using WppSender.Domain;
using Xunit;

namespace WppSender.Api.IntegrationTests;

public class CampanhasEndpointTests : IAsyncLifetime
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

    private async Task<Guid> CriarGrupoComLeadAsync()
    {
        var lead = await _client.PostAsJsonAsync("/api/leads", new
        {
            nome = "Lead Teste",
            telefone = "11911111111",
            instagram = (string?)null,
            endereco = (object?)null,
            origem = (string?)null,
            grupoId = (Guid?)null,
        });
        var leadCorpo = await lead.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var grupo = await _client.PostAsJsonAsync("/api/grupos", new
        {
            nome = "Grupo Teste",
            descricao = (string?)null,
            leadIds = new[] { leadCorpo!["id"] },
        });
        var grupoCorpo = await grupo.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        return grupoCorpo!["id"];
    }

    [Fact]
    public async Task DeveCriarCampanhaComEnviosPendentesEListar()
    {
        var grupoId = await CriarGrupoComLeadAsync();

        var criacao = await _client.PostAsJsonAsync("/api/campanhas", new
        {
            nome = "Campanha Teste",
            mensagem = "Olá {{nome}}",
            grupoId,
            agendadoPara = (DateTime?)null,
        });
        Assert.Equal(HttpStatusCode.OK, criacao.StatusCode);
        var corpo = await criacao.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var detalhe = await _client.GetAsync($"/api/campanhas/{corpo!["id"]}");
        Assert.Equal(HttpStatusCode.OK, detalhe.StatusCode);
        var detalheTexto = await detalhe.Content.ReadAsStringAsync();
        Assert.Contains("\"pendente\":1", detalheTexto.ToLowerInvariant());
    }

    [Fact]
    public async Task DeveRetornarBadRequest_QuandoGrupoSemLeadsAtivos()
    {
        var grupoVazio = await _client.PostAsJsonAsync("/api/grupos", new { nome = "Grupo Vazio Teste", descricao = (string?)null, leadIds = new Guid[0] });

        var resposta = await _client.PostAsJsonAsync("/api/campanhas", new
        {
            nome = "Campanha",
            mensagem = "Msg",
            grupoId = Guid.NewGuid(),
            agendadoPara = (DateTime?)null,
        });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task DeveRetornarConflict_QuandoEditarCampanhaEmAndamento()
    {
        var grupoId = await CriarGrupoComLeadAsync();
        var criacao = await _client.PostAsJsonAsync("/api/campanhas", new { nome = "Campanha", mensagem = "Msg", grupoId, agendadoPara = (DateTime?)null });
        var corpo = await criacao.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        using (var escopo = _factory.Services.CreateScope())
        {
            var sessaoRepositorio = escopo.ServiceProvider.GetRequiredService<ISessaoWhatsAppRepository>();
            var sessao = await sessaoRepositorio.ObterAsync();
            sessao.MarcarConectado();
            await sessaoRepositorio.AtualizarAsync(sessao);
        }

        var iniciar = await _client.PostAsync($"/api/campanhas/{corpo!["id"]}/iniciar", null);
        Assert.Equal(HttpStatusCode.OK, iniciar.StatusCode);

        var edicao = await _client.PutAsJsonAsync($"/api/campanhas/{corpo["id"]}", new { nome = "Novo Nome", mensagem = "Nova Msg", agendadoPara = (DateTime?)null, intervaloMinSegundos = 30, intervaloMaxSegundos = 90 });
        Assert.Equal(HttpStatusCode.Conflict, edicao.StatusCode);
    }

    [Fact]
    public async Task DeveRetornarBadRequest_QuandoIniciarSemSessaoConectada()
    {
        var grupoId = await CriarGrupoComLeadAsync();
        var criacao = await _client.PostAsJsonAsync("/api/campanhas", new { nome = "Campanha", mensagem = "Msg", grupoId, agendadoPara = (DateTime?)null });
        var corpo = await criacao.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var resposta = await _client.PostAsync($"/api/campanhas/{corpo!["id"]}/iniciar", null);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task DeveExcluirCampanhaEmRascunho()
    {
        var grupoId = await CriarGrupoComLeadAsync();
        var criacao = await _client.PostAsJsonAsync("/api/campanhas", new { nome = "Campanha", mensagem = "Msg", grupoId, agendadoPara = (DateTime?)null });
        var corpo = await criacao.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var exclusao = await _client.DeleteAsync($"/api/campanhas/{corpo!["id"]}");

        Assert.Equal(HttpStatusCode.OK, exclusao.StatusCode);
        var detalhe = await _client.GetAsync($"/api/campanhas/{corpo["id"]}");
        Assert.Equal(HttpStatusCode.NotFound, detalhe.StatusCode);
    }
}
