using WppSender.Domain;

namespace WppSender.Application.Leads;

public class ExportarLeadsCsvUseCase
{
    private readonly ILeadRepository _repositorio;
    private readonly ILeadCsvWriter _writer;
    private readonly int _tamanhoPagina;

    public ExportarLeadsCsvUseCase(ILeadRepository repositorio, ILeadCsvWriter writer, int tamanhoPagina = 500)
    {
        _repositorio = repositorio;
        _writer = writer;
        _tamanhoPagina = tamanhoPagina;
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
            var (itens, total) = await _repositorio.ListarAsync(busca: null, pagina, _tamanhoPagina);
            if (itens.Count == 0)
            {
                yield break;
            }

            foreach (var lead in itens)
            {
                yield return lead;
            }

            if (pagina * _tamanhoPagina >= total)
            {
                yield break;
            }

            pagina++;
        }
    }
}
