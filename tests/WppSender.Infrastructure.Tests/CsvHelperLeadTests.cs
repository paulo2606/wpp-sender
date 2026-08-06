using System.Text;
using WppSender.Application.Leads;
using WppSender.Domain;
using WppSender.Infrastructure.Csv;
using Xunit;

namespace WppSender.Infrastructure.Tests;

public class CsvHelperLeadTests
{
    [Fact]
    public void Parse_DeveLerTodasAsColunas_IncluindoEnderecoComVirgulaNoCampo()
    {
        var csvTexto = "nome,telefone,instagram,rua,numero,complemento,bairro,cidade,estado,cep,origem\n" +
                       "Fulano,11912345678,fulano_insta,\"Rua A, 123\",100,Apto 2,Centro,São Paulo,SP,01000-000,site\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvTexto));
        var parser = new CsvHelperLeadParser();

        var linhas = parser.Parse(stream).ToList();

        Assert.Single(linhas);
        Assert.Equal("Fulano", linhas[0].Nome);
        Assert.Equal("11912345678", linhas[0].Telefone);
        Assert.Equal("Rua A, 123", linhas[0].Rua);
        Assert.Equal("Apto 2", linhas[0].Complemento);
        Assert.Equal(2, linhas[0].NumeroLinha);
    }

    [Fact]
    public async Task EscreverAsync_DeveGerarCsvComCabecalhoECampoDoLead()
    {
        var writer = new CsvHelperLeadWriter();
        var endereco = new Endereco(Guid.NewGuid(), "Rua B", "200", null, "Bairro X", "Rio de Janeiro", "RJ", "20000-000");
        var lead = new Lead(Guid.NewGuid(), "Ciclana", "21988887777", "ciclana_insta", endereco, "indicacao");
        using var destino = new MemoryStream();

        await writer.EscreverAsync(destino, ParaAsyncEnumerable(lead));

        destino.Position = 0;
        var texto = new StreamReader(destino).ReadToEnd();
        Assert.Contains("nome,telefone,instagram,rua,numero,complemento,bairro,cidade,estado,cep,origem", texto);
        Assert.Contains("Ciclana", texto);
        Assert.Contains("21988887777", texto);
        Assert.Contains("Rio de Janeiro", texto);
    }

    [Fact]
    public async Task EscreverAsync_DeveSanitizarCampoQueComecaComCaractereDeFormula()
    {
        var writer = new CsvHelperLeadWriter();
        var lead = new Lead(Guid.NewGuid(), "=cmd|'/c calc'!A1", "11999999999", null, null, null);
        using var destino = new MemoryStream();

        await writer.EscreverAsync(destino, ParaAsyncEnumerable(lead));

        destino.Position = 0;
        var texto = new StreamReader(destino).ReadToEnd();
        Assert.Contains("'=cmd|'/c calc'!A1", texto);

        destino.Position = 0;
        var parser = new CsvHelperLeadParser();
        var linhas = parser.Parse(destino).ToList();
        Assert.Single(linhas);

        Assert.Equal("=cmd|'/c calc'!A1", linhas[0].Nome);
    }

    [Fact]
    public async Task EscreverAsync_DeveSanitizarCampoQueComecaComTabOuCarriageReturn()
    {
        var writer = new CsvHelperLeadWriter();
        var leadComTab = new Lead(Guid.NewGuid(), "\tNomeComTab", "11999999998", null, null, null);
        var leadComCr = new Lead(Guid.NewGuid(), "\rNomeComCr", "11999999997", null, null, null);
        using var destino = new MemoryStream();

        await writer.EscreverAsync(destino, ParaAsyncEnumerable(leadComTab, leadComCr));

        destino.Position = 0;
        var texto = new StreamReader(destino).ReadToEnd();
        Assert.Contains("'\tNomeComTab", texto);
        Assert.Contains("'\rNomeComCr", texto);
    }

    [Fact]
    public void Parse_NaoDeveRemoverApostrofo_QuandoNaoForOPrefixoDeSanitizacaoDoWriter()
    {
        var cabecalho = new[] { "nome", "telefone", "instagram", "rua", "numero", "complemento", "bairro", "cidade", "estado", "cep", "origem" };
        var valores = new[] { "\"'Twas a name\"", "11912345678", "", "", "", "", "", "", "", "", "" };
        var csvTexto = string.Join(',', cabecalho) + "\n" + string.Join(',', valores) + "\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvTexto));
        var parser = new CsvHelperLeadParser();

        var linhas = parser.Parse(stream).ToList();

        Assert.Single(linhas);
        Assert.Equal("'Twas a name", linhas[0].Nome);
    }

    [Fact]
    public void Parse_NaoDeveLancar_QuandoColunaOrigemAusenteDoCabecalho()
    {
        var cabecalho = new[] { "nome", "telefone", "instagram", "rua", "numero", "complemento", "bairro", "cidade", "estado", "cep" };
        var valores = new[] { "Fulano", "11912345678", "", "", "", "", "", "", "", "" };
        var csvTexto = string.Join(',', cabecalho) + "\n" + string.Join(',', valores) + "\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvTexto));
        var parser = new CsvHelperLeadParser();

        var linhas = parser.Parse(stream).ToList();

        Assert.Single(linhas);
        Assert.Equal("Fulano", linhas[0].Nome);
        Assert.Null(linhas[0].Origem);
    }

    [Fact]
    public void Parse_DeveRetornarVazio_QuandoStreamVazio()
    {
        using var stream = new MemoryStream();
        var parser = new CsvHelperLeadParser();

        var linhas = parser.Parse(stream).ToList();

        Assert.Empty(linhas);
    }

    [Fact]
    public async Task EscreverAsync_DeveIncluirNomeDoGrupoNaColuna()
    {
        var writer = new CsvHelperLeadWriter();
        var lead = new Lead(Guid.NewGuid(), "Fulano", "11912345678", null, null, null);
        using var destino = new MemoryStream();

        await writer.EscreverAsync(destino, ParaAsyncEnumerableComGrupo(new LeadExportavel(lead, "Clientes VIP")));

        destino.Position = 0;
        var texto = new StreamReader(destino).ReadToEnd();
        Assert.Contains("nome,telefone,instagram,rua,numero,complemento,bairro,cidade,estado,cep,origem,grupo", texto);
        Assert.Contains("Clientes VIP", texto);
    }

    [Fact]
    public async Task EscreverAsync_DeveSanitizarNomeDoGrupoQueComecaComCaractereDeFormula()
    {
        var writer = new CsvHelperLeadWriter();
        var lead = new Lead(Guid.NewGuid(), "Fulano", "11912345678", null, null, null);
        using var destino = new MemoryStream();

        await writer.EscreverAsync(destino, ParaAsyncEnumerableComGrupo(new LeadExportavel(lead, "=cmd|'/c calc'!A1")));

        destino.Position = 0;
        var texto = new StreamReader(destino).ReadToEnd();
        Assert.Contains("'=cmd|'/c calc'!A1", texto);
    }

    private static async IAsyncEnumerable<LeadExportavel> ParaAsyncEnumerableComGrupo(params LeadExportavel[] exportaveis)
    {
        foreach (var exportavel in exportaveis)
        {
            yield return exportavel;
        }

        await Task.CompletedTask;
    }

    private static async IAsyncEnumerable<LeadExportavel> ParaAsyncEnumerable(params Lead[] leads)
    {
        foreach (var lead in leads)
        {
            yield return new LeadExportavel(lead, null);
        }

        await Task.CompletedTask;
    }
}
