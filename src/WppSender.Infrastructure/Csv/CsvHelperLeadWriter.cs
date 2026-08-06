using System.Globalization;
using System.Text;
using CsvHelper;
using WppSender.Application.Leads;

namespace WppSender.Infrastructure.Csv;

public class CsvHelperLeadWriter : ILeadCsvWriter
{
    public async Task EscreverAsync(Stream destino, IAsyncEnumerable<LeadExportavel> leads)
    {

        await using var writer = new StreamWriter(destino, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), leaveOpen: true);
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
        csvWriter.WriteField("grupo");
        await csvWriter.NextRecordAsync();

        await foreach (var exportavel in leads)
        {
            var lead = exportavel.Lead;
            csvWriter.WriteField(SanitizarContraFormula(lead.Nome));
            csvWriter.WriteField(SanitizarContraFormula(lead.TelefoneNormalizado));
            csvWriter.WriteField(SanitizarContraFormula(lead.Instagram ?? string.Empty));
            csvWriter.WriteField(SanitizarContraFormula(lead.Endereco?.Rua ?? string.Empty));
            csvWriter.WriteField(SanitizarContraFormula(lead.Endereco?.Numero ?? string.Empty));
            csvWriter.WriteField(SanitizarContraFormula(lead.Endereco?.Complemento ?? string.Empty));
            csvWriter.WriteField(SanitizarContraFormula(lead.Endereco?.Bairro ?? string.Empty));
            csvWriter.WriteField(SanitizarContraFormula(lead.Endereco?.Cidade ?? string.Empty));
            csvWriter.WriteField(SanitizarContraFormula(lead.Endereco?.Estado ?? string.Empty));
            csvWriter.WriteField(SanitizarContraFormula(lead.Endereco?.Cep ?? string.Empty));
            csvWriter.WriteField(SanitizarContraFormula(lead.Origem ?? string.Empty));
            csvWriter.WriteField(SanitizarContraFormula(exportavel.NomeGrupo ?? string.Empty));
            await csvWriter.NextRecordAsync();
        }

        await writer.FlushAsync();
    }

    private static readonly char[] CaracteresDeFormula = ['=', '+', '-', '@', '\t', '\r'];

    private static string SanitizarContraFormula(string valor)
    {
        if (!string.IsNullOrEmpty(valor) && Array.IndexOf(CaracteresDeFormula, valor[0]) >= 0)
        {
            return "'" + valor;
        }

        return valor;
    }
}
