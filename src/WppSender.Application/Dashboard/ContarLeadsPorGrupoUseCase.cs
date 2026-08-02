using WppSender.Domain;

namespace WppSender.Application.Dashboard;

public record LeadsPorGrupo(Guid GrupoId, string NomeGrupo, int Quantidade);

public class ContarLeadsPorGrupoUseCase
{
    private readonly ILeadRepository _leadRepositorio;
    private readonly IGrupoRepository _grupoRepositorio;

    public ContarLeadsPorGrupoUseCase(ILeadRepository leadRepositorio, IGrupoRepository grupoRepositorio)
    {
        _leadRepositorio = leadRepositorio;
        _grupoRepositorio = grupoRepositorio;
    }

    public async Task<IReadOnlyList<LeadsPorGrupo>> ExecutarAsync()
    {
        var contagens = await _leadRepositorio.ContarAtivosPorGrupoAsync();
        if (contagens.Count == 0)
        {
            return Array.Empty<LeadsPorGrupo>();
        }

        var grupos = await _grupoRepositorio.ListarTodosAsync();
        var gruposPorId = grupos.ToDictionary(g => g.Id, g => g.Nome);

        return contagens
            .Where(c => gruposPorId.ContainsKey(c.Key))
            .Select(c => new LeadsPorGrupo(c.Key, gruposPorId[c.Key], c.Value))
            .OrderByDescending(l => l.Quantidade)
            .ToList();
    }
}
