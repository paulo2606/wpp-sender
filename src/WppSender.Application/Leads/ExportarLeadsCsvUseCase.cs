using WppSender.Domain;

namespace WppSender.Application.Leads;

public class ExportarLeadsCsvUseCase
{
    private const int TamanhoPagina = 500;

    private readonly ILeadRepository _repositorio;
    private readonly ILeadCsvWriter _writer;

    public ExportarLeadsCsvUseCase(ILeadRepository repositorio, ILeadCsvWriter writer)
    {
        _repositorio = repositorio;
        _writer = writer;
    }

    public async Task ExecutarAsync(Stream destino)
    {
        await _writer.EscreverAsync(destino, ObterTodosAtivosAsync());
    }

    private async IAsyncEnumerable<Lead> ObterTodosAtivosAsync()
    {
        var pagina = 1;
        while (true)
        {
            var (itens, total) = await _repositorio.ListarAsync(busca: null, pagina, TamanhoPagina);
            if (itens.Count == 0)
            {
                yield break;
            }

            foreach (var lead in itens)
            {
                yield return lead;
            }

            if (pagina * TamanhoPagina >= total)
            {
                yield break;
            }

            pagina++;
        }
    }
}
