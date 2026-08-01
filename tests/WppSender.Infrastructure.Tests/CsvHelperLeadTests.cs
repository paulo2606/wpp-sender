using System.Text;
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

    private static async IAsyncEnumerable<Lead> ParaAsyncEnumerable(params Lead[] leads)
    {
        foreach (var lead in leads)
        {
            yield return lead;
        }

        await Task.CompletedTask;
    }
}
