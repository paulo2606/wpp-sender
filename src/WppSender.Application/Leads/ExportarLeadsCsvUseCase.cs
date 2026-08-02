using WppSender.Domain;

namespace WppSender.Application.Leads;

public class ExportarLeadsCsvUseCase
{
    private readonly ILeadRepository _leadRepositorio;
    private readonly IGrupoRepository _grupoRepositorio;
    private readonly ILeadCsvWriter _writer;
    private readonly int _tamanhoPagina;

    public ExportarLeadsCsvUseCase(ILeadRepository leadRepositorio, IGrupoRepository grupoRepositorio, ILeadCsvWriter writer, int tamanhoPagina = 500)
    {
        _leadRepositorio = leadRepositorio;
        _grupoRepositorio = grupoRepositorio;
        _writer = writer;
        _tamanhoPagina = tamanhoPagina;
    }

    public async Task ExecutarAsync(Stream destino, Guid? grupoId = null)
    {
        var gruposPorId = (await _grupoRepositorio.ListarTodosAsync()).ToDictionary(g => g.Id, g => g.Nome);

        await _writer.EscreverAsync(destino, ObterTodosAtivosAsync(grupoId, gruposPorId));
    }

    private async IAsyncEnumerable<LeadExportavel> ObterTodosAtivosAsync(Guid? grupoId, IReadOnlyDictionary<Guid, string> gruposPorId)
    {
        var pagina = 1;
        while (true)
        {
            var (itens, total) = await _leadRepositorio.ListarAsync(busca: null, pagina, _tamanhoPagina, grupoId);
            if (itens.Count == 0)
            {
                yield break;
            }

            foreach (var lead in itens)
            {
                var nomeGrupo = lead.GrupoId is not null && gruposPorId.TryGetValue(lead.GrupoId.Value, out var nome)
                    ? nome
                    : null;

                yield return new LeadExportavel(lead, nomeGrupo);
            }

            if (pagina * _tamanhoPagina >= total)
            {
                yield break;
            }

            pagina++;
        }
    }
}
