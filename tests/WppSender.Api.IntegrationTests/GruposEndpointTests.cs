using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Xunit;

namespace WppSender.Api.IntegrationTests;

public class GruposEndpointTests : IAsyncLifetime
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

    private async Task<Guid> CriarLeadAsync(string nome, string telefone)
    {
        var resposta = await _client.PostAsJsonAsync("/api/leads", new
        {
            nome,
            telefone,
            instagram = (string?)null,
            endereco = (object?)null,
            origem = (string?)null,
            grupoId = (Guid?)null
        });
        var corpo = await resposta.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        return corpo!["id"];
    }

    [Fact]
    public async Task DeveCriarGrupoComLeadsEListarComContagem()
    {
        var lead1 = await CriarLeadAsync("Lead1", "11911111111");
        var lead2 = await CriarLeadAsync("Lead2", "11922222222");

        var respostaCriacao = await _client.PostAsJsonAsync("/api/grupos", new { nome = "Clientes VIP", descricao = "Descricao", leadIds = new[] { lead1, lead2 } });
        Assert.Equal(HttpStatusCode.OK, respostaCriacao.StatusCode);

        var listagem = await _client.GetFromJsonAsync<ListaGruposResponseTeste>("/api/grupos");
        Assert.Equal(1, listagem!.Total);
        Assert.Equal(2, listagem.Itens[0].QuantidadeLeads);
    }

    [Fact]
    public async Task DeveRetornarBadRequest_QuandoCriaGrupoSemLeads()
    {
        var resposta = await _client.PostAsJsonAsync("/api/grupos", new { nome = "Grupo Vazio", descricao = (string?)null, leadIds = Array.Empty<Guid>() });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task DeveRetornarBadRequest_QuandoAlgumLeadNaoExiste()
    {
        var lead1 = await CriarLeadAsync("Lead1", "11933333333");

        var resposta = await _client.PostAsJsonAsync("/api/grupos", new { nome = "Grupo", descricao = (string?)null, leadIds = new[] { lead1, Guid.NewGuid() } });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task DeveDesvincularLeads_QuandoGrupoExcluido()
    {
        var lead1 = await CriarLeadAsync("Lead1", "11944444444");
        var criacao = await _client.PostAsJsonAsync("/api/grupos", new { nome = "Grupo Temporario", descricao = (string?)null, leadIds = new[] { lead1 } });
        var grupoCriado = await criacao.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var exclusao = await _client.DeleteAsync($"/api/grupos/{grupoCriado!["id"]}");
        Assert.Equal(HttpStatusCode.OK, exclusao.StatusCode);

        var leadObtido = await _client.GetFromJsonAsync<LeadResponseTeste>($"/api/leads/{lead1}");
        Assert.Null(leadObtido!.GrupoId);
    }

    [Fact]
    public async Task DeveEditarGrupo()
    {
        var lead1 = await CriarLeadAsync("Lead1", "11955555555");
        var criacao = await _client.PostAsJsonAsync("/api/grupos", new { nome = "Nome Antigo", descricao = (string?)null, leadIds = new[] { lead1 } });
        var grupoCriado = await criacao.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var edicao = await _client.PutAsJsonAsync($"/api/grupos/{grupoCriado!["id"]}", new { nome = "Nome Novo", descricao = "Nova Descricao" });
        Assert.Equal(HttpStatusCode.OK, edicao.StatusCode);

        var listagem = await _client.GetFromJsonAsync<ListaGruposResponseTeste>("/api/grupos");
        Assert.Equal("Nome Novo", listagem!.Itens[0].Nome);
    }

    [Fact]
    public async Task DeveAdicionarLeadAoGrupo_QuandoEditarComLeadIds()
    {
        var lead1 = await CriarLeadAsync("Lead1", "11955556666");
        var lead2 = await CriarLeadAsync("Lead2", "11955557777");
        var criacao = await _client.PostAsJsonAsync("/api/grupos", new { nome = "Grupo", descricao = (string?)null, leadIds = new[] { lead1 } });
        var grupoCriado = await criacao.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var edicao = await _client.PutAsJsonAsync($"/api/grupos/{grupoCriado!["id"]}", new { nome = "Grupo", descricao = (string?)null, leadIds = new[] { lead2 } });
        Assert.Equal(HttpStatusCode.OK, edicao.StatusCode);

        var leadObtido = await _client.GetFromJsonAsync<LeadResponseTeste>($"/api/leads/{lead2}");
        Assert.Equal(grupoCriado["id"], leadObtido!.GrupoId);
        var listagem = await _client.GetFromJsonAsync<ListaGruposResponseTeste>("/api/grupos");
        Assert.Equal(2, listagem!.Itens.Single(g => g.Id == grupoCriado["id"]).QuantidadeLeads);
    }

    [Fact]
    public async Task DeveRetornarNotFound_QuandoEditarGrupoInexistente()
    {
        var resposta = await _client.PutAsJsonAsync($"/api/grupos/{Guid.NewGuid()}", new { nome = "Nome", descricao = (string?)null });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task DeveRetornarBadRequest_QuandoEditarGrupoComNomeInvalido()
    {
        var lead1 = await CriarLeadAsync("Lead1", "11977777777");
        var criacao = await _client.PostAsJsonAsync("/api/grupos", new { nome = "Nome Valido", descricao = (string?)null, leadIds = new[] { lead1 } });
        var grupoCriado = await criacao.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var edicao = await _client.PutAsJsonAsync($"/api/grupos/{grupoCriado!["id"]}", new { nome = "", descricao = (string?)null });

        Assert.Equal(HttpStatusCode.BadRequest, edicao.StatusCode);
    }

    [Fact]
    public async Task DeveRetornarNotFound_QuandoExcluirGrupoInexistente()
    {
        var resposta = await _client.DeleteAsync($"/api/grupos/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task DeveExportarCsvComColunaDeGrupo()
    {
        var lead1 = await CriarLeadAsync("ExportComGrupo", "11966666666");
        await _client.PostAsJsonAsync("/api/grupos", new { nome = "Grupo Export", descricao = (string?)null, leadIds = new[] { lead1 } });

        var resposta = await _client.GetAsync("/api/leads/exportar");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var texto = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("grupo", texto);
        Assert.Contains("Grupo Export", texto);
    }

    private record ListaGruposResponseTeste(List<GrupoResponseTeste> Itens, int Total, int Pagina, int TamanhoPagina);
    private record GrupoResponseTeste(Guid Id, string Nome, string? Descricao, int QuantidadeLeads);
    private record LeadResponseTeste(Guid Id, string Nome, Guid? GrupoId);
}
