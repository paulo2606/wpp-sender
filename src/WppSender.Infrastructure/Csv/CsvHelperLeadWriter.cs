using System.Globalization;
using CsvHelper;
using WppSender.Application.Leads;
using WppSender.Domain;

namespace WppSender.Infrastructure.Csv;

public class CsvHelperLeadWriter : ILeadCsvWriter
{
    public async Task EscreverAsync(Stream destino, IAsyncEnumerable<Lead> leads)
    {
        await using var writer = new StreamWriter(destino, leaveOpen: true);
        await using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csvWriter.WriteField("nome");
        csvWriter.WriteField("telefone");
        csvWriter.WriteField("instagram");
        csvWriter.WriteField("rua");
        csvWriter.WriteField("numero");
        csvWriter.WriteField("complemento");
        csvWriter.WriteField("bairro");
        csvWriter.WriteField("cidade");
        csvWriter.WriteField("estado");
        csvWriter.WriteField("cep");
        csvWriter.WriteField("origem");
        await csvWriter.NextRecordAsync();

        await foreach (var lead in leads)
        {
            csvWriter.WriteField(lead.Nome);
            csvWriter.WriteField(lead.TelefoneNormalizado);
            csvWriter.WriteField(lead.Instagram ?? string.Empty);
            csvWriter.WriteField(lead.Endereco?.Rua ?? string.Empty);
            csvWriter.WriteField(lead.Endereco?.Numero ?? string.Empty);
            csvWriter.WriteField(lead.Endereco?.Complemento ?? string.Empty);
            csvWriter.WriteField(lead.Endereco?.Bairro ?? string.Empty);
            csvWriter.WriteField(lead.Endereco?.Cidade ?? string.Empty);
            csvWriter.WriteField(lead.Endereco?.Estado ?? string.Empty);
            csvWriter.WriteField(lead.Endereco?.Cep ?? string.Empty);
            csvWriter.WriteField(lead.Origem ?? string.Empty);
            await csvWriter.NextRecordAsync();
        }

        await writer.FlushAsync();
    }
}
