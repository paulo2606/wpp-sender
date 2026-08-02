using WppSender.Domain;

namespace WppSender.Application.Tests.Fakes;

public class FakeEnvioRepository : IEnvioRepository
{
    private readonly List<Envio> _envios = new();

    public Task AdicionarVariosAsync(IReadOnlyList<Envio> envios)
    {
        _envios.AddRange(envios);
        return Task.CompletedTask;
    }

    public Task AtualizarAsync(Envio envio) => Task.CompletedTask;

    public Task<Envio?> BuscarProximoPendenteAsync(Guid campanhaId)
    {
        var resultado = _envios
            .Where(e => e.CampanhaId == campanhaId && e.Status == StatusEnvio.Pendente)
            .OrderBy(e => e.Id)
            .FirstOrDefault();

        return Task.FromResult(resultado);
    }

    public Task<IReadOnlyDictionary<StatusEnvio, int>> ContarPorStatusAsync(Guid campanhaId)
    {
        var contagens = _envios
            .Where(e => e.CampanhaId == campanhaId)
            .GroupBy(e => e.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        return Task.FromResult<IReadOnlyDictionary<StatusEnvio, int>>(contagens);
    }

    public Task<IReadOnlyList<Envio>> ListarFalhosAsync(Guid campanhaId)
    {
        var resultado = _envios
            .Where(e => e.CampanhaId == campanhaId && e.Status == StatusEnvio.Falhou)
            .OrderBy(e => e.Id)
            .ToList();

        return Task.FromResult<IReadOnlyList<Envio>>(resultado);
    }

    // Exposto pros testes do motor de envio (Task 6) inspecionarem o estado bruto.
    public IReadOnlyList<Envio> Todos => _envios;
}
