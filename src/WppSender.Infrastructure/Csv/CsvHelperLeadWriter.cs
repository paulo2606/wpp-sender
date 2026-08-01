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
            await csvWriter.NextRecordAsync();
        }

        await writer.FlushAsync();
    }

    // Mitiga CSV Formula Injection: se o valor começar com um dos caracteres
    // que planilhas (Excel, Google Sheets, LibreOffice Calc) interpretam como
    // início de fórmula, prefixa com aspas simples para forçar leitura como texto.
    private static readonly char[] CaracteresDeFormula = ['=', '+', '-', '@'];

    private static string SanitizarContraFormula(string valor)
    {
        if (!string.IsNullOrEmpty(valor) && Array.IndexOf(CaracteresDeFormula, valor[0]) >= 0)
        {
            return "'" + valor;
        }

        return valor;
    }
}
