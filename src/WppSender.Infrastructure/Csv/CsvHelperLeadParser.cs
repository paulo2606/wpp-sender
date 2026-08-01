using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using WppSender.Application.Leads;

namespace WppSender.Infrastructure.Csv;

public class CsvHelperLeadParser : ILeadCsvParser
{
    // Mesmo conjunto de caracteres usado pelo CsvHelperLeadWriter para sanitizar contra
    // CSV Formula Injection. Usado aqui só para reconhecer e remover o prefixo de aspas
    // simples que o writer adiciona, evitando que ele se acumule a cada ciclo de
    // exportação seguida de reimportação.
    private static readonly char[] CaracteresDeFormula = ['=', '+', '-', '@', '\t', '\r'];

    public IEnumerable<LeadCsvLinha> Parse(Stream csv)
    {
        using var reader = new StreamReader(csv, leaveOpen: true);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            MissingFieldFound = null,
        };
        using var csvReader = new CsvReader(reader, config);

        if (!csvReader.Read())
        {
            yield break;
        }

        csvReader.ReadHeader();

        var numeroLinha = 1;
        while (csvReader.Read())
        {
            numeroLinha++;
            yield return new LeadCsvLinha(
                NumeroLinha: numeroLinha,
                Nome: RemoverPrefixoDeSanitizacao(csvReader.GetField("nome")) ?? string.Empty,
                Telefone: RemoverPrefixoDeSanitizacao(csvReader.GetField("telefone")) ?? string.Empty,
                Instagram: RemoverPrefixoDeSanitizacao(csvReader.GetField("instagram")),
                Rua: RemoverPrefixoDeSanitizacao(csvReader.GetField("rua")),
                Numero: RemoverPrefixoDeSanitizacao(csvReader.GetField("numero")),
                Complemento: RemoverPrefixoDeSanitizacao(csvReader.GetField("complemento")),
                Bairro: RemoverPrefixoDeSanitizacao(csvReader.GetField("bairro")),
                Cidade: RemoverPrefixoDeSanitizacao(csvReader.GetField("cidade")),
                Estado: RemoverPrefixoDeSanitizacao(csvReader.GetField("estado")),
                Cep: RemoverPrefixoDeSanitizacao(csvReader.GetField("cep")),
                Origem: RemoverPrefixoDeSanitizacao(csvReader.GetField("origem")));
        }
    }

    // Remove apenas o prefixo que o próprio sanitizador do writer adiciona (aspas simples
    // seguida imediatamente de um caractere de fórmula). Um apóstrofo real no início de um
    // nome (ex.: "O'Brien") não é seguido por um desses caracteres e permanece intacto.
    private static string? RemoverPrefixoDeSanitizacao(string? valor)
    {
        if (valor is not null && valor.Length >= 2 && valor[0] == '\'' && Array.IndexOf(CaracteresDeFormula, valor[1]) >= 0)
        {
            return valor[1..];
        }

        return valor;
    }
}
