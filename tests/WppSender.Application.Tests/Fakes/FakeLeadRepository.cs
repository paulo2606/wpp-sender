using WppSender.Domain;

namespace WppSender.Application.Tests.Fakes;

public class FakeLeadRepository : ILeadRepository
{
    private readonly List<Lead> _leads = new();
    public int ChamadasBuscarPorTelefone { get; private set; }

    public Task<Lead?> BuscarPorTelefoneNormalizadoAsync(string telefoneNormalizado)
    {
        ChamadasBuscarPorTelefone++;
        return Task.FromResult(_leads.FirstOrDefault(l => l.TelefoneNormalizado == telefoneNormalizado && l.EstaAtivo));
    }

    public Task<Lead?> BuscarPorIdAsync(Guid id)
        => Task.FromResult(_leads.FirstOrDefault(l => l.Id == id));

    public Task AdicionarAsync(Lead lead)
    {
        _leads.Add(lead);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Lead lead) => Task.CompletedTask;

    public Task<(IReadOnlyList<Lead> Itens, int Total)> ListarAsync(string? busca, int pagina, int tamanhoPagina, Guid? grupoId = null)
    {
        var query = _leads.Where(l => l.EstaAtivo);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            query = query.Where(l => l.Nome.Contains(busca) || l.TelefoneNormalizado.Contains(busca));
        }

        if (grupoId is not null)
        {
            query = query.Where(l => l.GrupoId == grupoId);
        }

        var todos = query.OrderBy(l => l.Nome).ThenBy(l => l.Id).ToList();
        var pagina2 = todos.Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina).ToList();

        return Task.FromResult<(IReadOnlyList<Lead>, int)>((pagina2, todos.Count));
    }

    public int ContarAtivosPorGrupo(Guid grupoId) => _leads.Count(l => l.GrupoId == grupoId && l.EstaAtivo);

    public Task<IReadOnlyList<Lead>> ListarAtivosPorGrupoAsync(Guid grupoId)
    {
        var resultado = _leads
            .Where(l => l.GrupoId == grupoId && l.EstaAtivo)
            .OrderBy(l => l.Nome).ThenBy(l => l.Id)
            .ToList();

        return Task.FromResult<IReadOnlyList<Lead>>(resultado);
    }

    public Task<int> ContarAtivosCriadosDesdeAsync(DateTime desde)
    {
        var resultado = _leads.Count(l => l.EstaAtivo && l.CriadoEm >= desde);

        return Task.FromResult(resultado);
    }

    public Task<IReadOnlyDictionary<Guid, int>> ContarAtivosPorGrupoAsync()
    {
        var contagens = _leads
            .Where(l => l.EstaAtivo && l.GrupoId != null)
            .GroupBy(l => l.GrupoId!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        return Task.FromResult<IReadOnlyDictionary<Guid, int>>(contagens);
    }
}
