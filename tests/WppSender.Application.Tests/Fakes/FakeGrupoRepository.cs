using WppSender.Domain;

namespace WppSender.Application.Tests.Fakes;

public class FakeGrupoRepository : IGrupoRepository
{
    private readonly List<Grupo> _grupos = new();
    private readonly FakeLeadRepository? _leadRepositorio;

    public FakeGrupoRepository(FakeLeadRepository? leadRepositorio = null)
    {
        _leadRepositorio = leadRepositorio;
    }

    public Task<Grupo?> BuscarPorIdAsync(Guid id)
        => Task.FromResult(_grupos.FirstOrDefault(g => g.Id == id));

    public Task AdicionarAsync(Grupo grupo)
    {
        _grupos.Add(grupo);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Grupo grupo) => Task.CompletedTask;

    public Task RemoverAsync(Grupo grupo)
    {
        _grupos.Remove(grupo);
        return Task.CompletedTask;
    }

    public Task<(IReadOnlyList<(Grupo Grupo, int QuantidadeLeads)> Itens, int Total)> ListarComContagemAsync(int pagina, int tamanhoPagina)
    {
        var todos = _grupos.OrderBy(g => g.Nome).ThenBy(g => g.Id).ToList();
        var pagina2 = todos
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(g => (g, _leadRepositorio?.ContarAtivosPorGrupo(g.Id) ?? 0))
            .ToList();

        return Task.FromResult<(IReadOnlyList<(Grupo, int)>, int)>((pagina2, todos.Count));
    }

    public Task<IReadOnlyList<Grupo>> ListarTodosAsync()
        => Task.FromResult<IReadOnlyList<Grupo>>(_grupos.ToList());
}
