using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using WppSender.Domain;
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
        login.EnsureSuccessStatusCode();
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
    public async Task DeveEditarEndereco_EGetRetornarOsNovosDados()
    {
        var criado = await _client.PostAsJsonAsync("/api/leads", new
        {
            nome = "Fulano",
            telefone = "11966666666",
            instagram = (string?)null,
            endereco = new { rua = "Rua A", numero = "100", complemento = (string?)null, bairro = "Centro", cidade = "São Paulo", estado = "SP", cep = "01000-000" },
            origem = (string?)null
        });
        var corpo = await criado.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var id = corpo!["id"];

        var edicao = await _client.PutAsJsonAsync($"/api/leads/{id}", new
        {
            nome = "Fulano Editado",
            telefone = "11966666666",
            instagram = (string?)null,
            endereco = new { rua = "Rua B", numero = "200", complemento = "Apto 3", bairro = "Jardins", cidade = "Rio de Janeiro", estado = "RJ", cep = "20000-000" },
            origem = (string?)null
        });
        Assert.Equal(HttpStatusCode.OK, edicao.StatusCode);

        var obtido = await _client.GetFromJsonAsync<LeadResponseTeste>($"/api/leads/{id}");

        Assert.NotNull(obtido);
        Assert.Equal("Fulano Editado", obtido!.Nome);
        Assert.Equal("Rua B", obtido.Rua);
        Assert.Equal("200", obtido.Numero);
        Assert.Equal("Apto 3", obtido.Complemento);
        Assert.Equal("Jardins", obtido.Bairro);
        Assert.Equal("Rio de Janeiro", obtido.Cidade);
        Assert.Equal("RJ", obtido.Estado);
        Assert.Equal("20000-000", obtido.Cep);
    }

    [Fact]
    public async Task DeveRetornar404_QuandoLeadNaoExiste()
    {
        var resposta = await _client.GetAsync($"/api/leads/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task DeveRetornarOk_QuandoPaginaZeroOuNegativa()
    {
        await _client.PostAsJsonAsync("/api/leads", new { nome = "Fulano", telefone = "11977777777", instagram = (string?)null, endereco = (object?)null, origem = (string?)null });

        var respostaZero = await _client.GetAsync("/api/leads?pagina=0");
        var respostaNegativa = await _client.GetAsync("/api/leads?pagina=-5");

        Assert.Equal(HttpStatusCode.OK, respostaZero.StatusCode);
        Assert.Equal(HttpStatusCode.OK, respostaNegativa.StatusCode);
    }

    [Fact]
    public async Task DeveRetornarTamanhoPaginaEfetivo_QuandoTamanhoPaginaOversized()
    {
        var listagem = await _client.GetFromJsonAsync<ListaLeadsResponseTeste>("/api/leads?tamanhoPagina=1000");

        Assert.NotNull(listagem);
        Assert.Equal(100, listagem!.TamanhoPagina);
    }

    [Fact]
    public async Task DeveRetornarBadRequest_QuandoImportarSemArquivo()
    {
        using var conteudo = new MultipartFormDataContent();

        var resposta = await _client.PostAsync("/api/leads/importar", conteudo);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task DeveRetornarBadRequest_QuandoImportarComArquivoVazio()
    {
        using var conteudo = new MultipartFormDataContent();
        var arquivo = new ByteArrayContent(Array.Empty<byte>());
        arquivo.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        conteudo.Add(arquivo, "arquivo", "leads.csv");

        var resposta = await _client.PostAsync("/api/leads/importar", conteudo);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task DeveRejeitarImportacao_QuandoArquivoExcedeTamanhoMaximo()
    {
        using var conteudo = new MultipartFormDataContent();
        var bytesGrandes = new byte[6 * 1024 * 1024];
        var arquivo = new ByteArrayContent(bytesGrandes);
        arquivo.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        conteudo.Add(arquivo, "arquivo", "leads.csv");

        var resposta = await _client.PostAsync("/api/leads/importar", conteudo);

        Assert.NotEqual(HttpStatusCode.OK, resposta.StatusCode);
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

    [Fact]
    public async Task DeveCriarLeadComGrupoIdEFiltrarNaListagemPorGrupo()
    {
        using var scope = _factory.Services.CreateScope();
        var grupoRepositorio = scope.ServiceProvider.GetRequiredService<IGrupoRepository>();
        var grupo = new Grupo(Guid.NewGuid(), "Grupo Teste", null);
        await grupoRepositorio.AdicionarAsync(grupo);

        var criado = await _client.PostAsJsonAsync("/api/leads", new
        {
            nome = "ComGrupoReal",
            telefone = "11988887777",
            instagram = (string?)null,
            endereco = (object?)null,
            origem = (string?)null,
            grupoId = grupo.Id
        });
        Assert.Equal(HttpStatusCode.OK, criado.StatusCode);
        var corpo = await criado.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var id = corpo!["id"];

        var obtido = await _client.GetFromJsonAsync<LeadResponseTeste>($"/api/leads/{id}");
        Assert.NotNull(obtido);
        Assert.Equal(grupo.Id, obtido!.GrupoId);

        var listagemFiltrada = await _client.GetFromJsonAsync<ListaLeadsResponseTeste>($"/api/leads?grupoId={grupo.Id}");
        Assert.Equal(1, listagemFiltrada!.Total);
        Assert.Equal(id, listagemFiltrada.Itens[0].Id);
    }

    private record ListaLeadsResponseTeste(List<LeadResponseTeste> Itens, int Total, int Pagina, int TamanhoPagina);

    private record LeadResponseTeste(
        Guid Id,
        string Nome,
        string Telefone,
        string? Instagram,
        string? Origem,
        string? Rua,
        string? Numero,
        string? Complemento,
        string? Bairro,
        string? Cidade,
        string? Estado,
        string? Cep,
        Guid? GrupoId);

    private record ImportarLeadsResponseTeste(int Importados, List<object> Puladas);
}
