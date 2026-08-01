using System.Globalization;
using CsvHelper;
using WppSender.Application.Leads;

namespace WppSender.Infrastructure.Csv;

public class CsvHelperLeadParser : ILeadCsvParser
{
    public IEnumerable<LeadCsvLinha> Parse(Stream csv)
    {
        using var reader = new StreamReader(csv, leaveOpen: true);
        using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

        csvReader.Read();
        csvReader.ReadHeader();

        var numeroLinha = 1;
        while (csvReader.Read())
        {
            numeroLinha++;
            yield return new LeadCsvLinha(
                NumeroLinha: numeroLinha,
                Nome: csvReader.GetField("nome") ?? string.Empty,
                Telefone: csvReader.GetField("telefone") ?? string.Empty,
                Instagram: csvReader.GetField("instagram"),
                Rua: csvReader.GetField("rua"),
                Numero: csvReader.GetField("numero"),
                Complemento: csvReader.GetField("complemento"),
                Bairro: csvReader.GetField("bairro"),
                Cidade: csvReader.GetField("cidade"),
                Estado: csvReader.GetField("estado"),
                Cep: csvReader.GetField("cep"),
                Origem: csvReader.GetField("origem"));
        }
    }
}
