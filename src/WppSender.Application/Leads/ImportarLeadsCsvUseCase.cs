using WppSender.Domain;

namespace WppSender.Application.Leads;

public class ImportarLeadsCsvUseCase
{
    private const string MotivoTelefoneDuplicado = "Telefone já cadastrado";

    private readonly ILeadRepository _repositorio;
    private readonly ILeadCsvParser _parser;

    public ImportarLeadsCsvUseCase(ILeadRepository repositorio, ILeadCsvParser parser)
    {
        _repositorio = repositorio;
        _parser = parser;
    }

    public async Task<ImportarLeadsResultado> ExecutarAsync(Stream csv)
    {
        var importados = 0;
        var puladas = new List<LeadPulado>();
        var telefonesNesteImport = new HashSet<string>();

        foreach (var linha in _parser.Parse(csv))
        {
            var telefoneNormalizado = Lead.NormalizarTelefone(linha.Telefone);

            if (!telefonesNesteImport.Add(telefoneNormalizado))
            {
                puladas.Add(new LeadPulado(linha.NumeroLinha, linha.Telefone, MotivoTelefoneDuplicado));
                continue;
            }

            var existente = await _repositorio.BuscarPorTelefoneNormalizadoAsync(telefoneNormalizado);
            if (existente is not null)
            {
                puladas.Add(new LeadPulado(linha.NumeroLinha, linha.Telefone, MotivoTelefoneDuplicado));
                continue;
            }

            var temEndereco = !string.IsNullOrWhiteSpace(linha.Rua);
            var endereco = temEndereco
                ? new Endereco(Guid.NewGuid(), linha.Rua!, linha.Numero ?? string.Empty, linha.Complemento, linha.Bairro ?? string.Empty, linha.Cidade ?? string.Empty, linha.Estado ?? string.Empty, linha.Cep ?? string.Empty)
                : null;

            var lead = new Lead(Guid.NewGuid(), linha.Nome, linha.Telefone, linha.Instagram, endereco, linha.Origem);
            await _repositorio.AdicionarAsync(lead);
            importados++;
        }

        return new ImportarLeadsResultado(importados, puladas);
    }
}
