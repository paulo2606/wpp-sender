using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Xunit;

namespace WppSender.Api.IntegrationTests;

public class LeadsEndpointTests : IAsyncLifetime
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

    [Fact]
    public async Task DeveCriarELerLead_ComEnderecoEstruturado()
    {
        var resposta = await _client.PostAsJsonAsync("/api/leads", new
        {
            nome = "Fulano",
            telefone = "11912345678",
            instagram = "fulano_insta",
            endereco = new { rua = "Rua A", numero = "100", complemento = (string?)null, bairro = "Centro", cidade = "São Paulo", estado = "SP", cep = "01000-000" },
            origem = "site"
        });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var listagem = await _client.GetFromJsonAsync<ListaLeadsResponseTeste>("/api/leads");
        Assert.Equal(1, listagem!.Total);
        Assert.Equal("Fulano", listagem.Itens[0].Nome);
    }

    [Fact]
    public async Task DeveRetornarConflito_QuandoCriaComTelefoneDuplicado()
    {
        await _client.PostAsJsonAsync("/api/leads", new { nome = "Um", telefone = "11911111111", instagram = (string?)null, endereco = (object?)null, origem = (string?)null });

        var resposta = await _client.PostAsJsonAsync("/api/leads", new { nome = "Dois", telefone = "11911111111", instagram = (string?)null, endereco = (object?)null, origem = (string?)null });

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task DeveExcluirLead_ENaoApareceMaisNaListagem()
    {
        var criado = await _client.PostAsJsonAsync("/api/leads", new { nome = "ParaExcluir", telefone = "11933333333", instagram = (string?)null, endereco = (object?)null, origem = (string?)null });
        var corpo = await criado.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();

        var respostaExclusao = await _client.DeleteAsync($"/api/leads/{corpo!["id"]}");
        Assert.Equal(HttpStatusCode.OK, respostaExclusao.StatusCode);

        var listagem = await _client.GetFromJsonAsync<ListaLeadsResponseTeste>("/api/leads");
        Assert.Equal(0, listagem!.Total);
    }

    [Fact]
    public async Task DeveImportarCsv_EPularDuplicado()
    {
        var csvTexto = "nome,telefone,instagram,rua,numero,complemento,bairro,cidade,estado,cep,origem\n" +
                       "Importado1,11944444444,,,,,,,,,\n" +
                       "Importado1Dup,11944444444,,,,,,,,,\n";
        using var conteudo = new MultipartFormDataContent();
        var arquivo = new ByteArrayContent(Encoding.UTF8.GetBytes(csvTexto));
        arquivo.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        conteudo.Add(arquivo, "arquivo", "leads.csv");

        var resposta = await _client.PostAsync("/api/leads/importar", conteudo);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var resultado = await resposta.Content.ReadFromJsonAsync<ImportarLeadsResponseTeste>();
        Assert.Equal(1, resultado!.Importados);
        Assert.Single(resultado.Puladas);
    }

    [Fact]
    public async Task DeveExportarCsv_ComOsLeadsCriados()
    {
        await _client.PostAsJsonAsync("/api/leads", new { nome = "ParaExportar", telefone = "11955555555", instagram = (string?)null, endereco = (object?)null, origem = (string?)null });

        var resposta = await _client.GetAsync("/api/leads/exportar");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var texto = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("ParaExportar", texto);
        Assert.Contains("11955555555", texto);
    }

    private record ListaLeadsResponseTeste(List<LeadResponseTeste> Itens, int Total, int Pagina, int TamanhoPagina);
    private record LeadResponseTeste(Guid Id, string Nome, string Telefone, string? Instagram, string? Origem);
    private record ImportarLeadsResponseTeste(int Importados, List<object> Puladas);
}
