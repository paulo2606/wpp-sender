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

    public Task<(IReadOnlyList<Lead> Itens, int Total)> ListarAsync(string? busca, int pagina, int tamanhoPagina)
    {
        var query = _leads.Where(l => l.EstaAtivo);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            query = query.Where(l => l.Nome.Contains(busca) || l.TelefoneNormalizado.Contains(busca));
        }

        var todos = query.OrderBy(l => l.Nome).ThenBy(l => l.Id).ToList();
        var pagina2 = todos.Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina).ToList();

        return Task.FromResult<(IReadOnlyList<Lead>, int)>((pagina2, todos.Count));
    }
}
