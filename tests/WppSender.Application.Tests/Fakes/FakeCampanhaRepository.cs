using WppSender.Domain;

namespace WppSender.Application.Tests.Fakes;

public class FakeCampanhaRepository : ICampanhaRepository
{
    private readonly List<Campanha> _campanhas = new();

    public Task<Campanha?> BuscarPorIdAsync(Guid id)
        => Task.FromResult(_campanhas.FirstOrDefault(c => c.Id == id));

    public Task AdicionarAsync(Campanha campanha)
    {
        _campanhas.Add(campanha);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Campanha campanha) => Task.CompletedTask;

    public Task RemoverAsync(Campanha campanha)
    {
        _campanhas.Remove(campanha);
        return Task.CompletedTask;
    }

    public Task<(IReadOnlyList<Campanha> Itens, int Total)> ListarAsync(StatusCampanha? status, int pagina, int tamanhoPagina)
    {
        var query = _campanhas.AsEnumerable();

        if (status is not null)
        {
            query = query.Where(c => c.Status == status);
        }

        var todos = query.OrderBy(c => c.Nome).ThenBy(c => c.Id).ToList();
        var pagina2 = todos.Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina).ToList();

        return Task.FromResult<(IReadOnlyList<Campanha>, int)>((pagina2, todos.Count));
    }

    public Task<IReadOnlyList<Campanha>> ListarAgendadasParaIniciarAsync(DateTime agora)
    {
        var resultado = _campanhas
            .Where(c => c.Status == StatusCampanha.Agendada && c.AgendadoPara != null && c.AgendadoPara <= agora)
            .ToList();

        return Task.FromResult<IReadOnlyList<Campanha>>(resultado);
    }
}
