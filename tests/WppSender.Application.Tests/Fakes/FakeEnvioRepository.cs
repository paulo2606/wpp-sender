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

    public Task<IReadOnlyDictionary<StatusEnvio, int>> ContarTodosPorStatusAsync()
    {
        var contagens = _envios
            .GroupBy(e => e.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        return Task.FromResult<IReadOnlyDictionary<StatusEnvio, int>>(contagens);
    }

    public IReadOnlyList<Envio> Todos => _envios;

    public Dictionary<Guid, StatusCampanha> StatusCampanhas { get; } = new();

    public Task<IReadOnlyList<Envio>> ListarAguardandoConfirmacaoAsync()
    {
        var resultado = _envios
            .Where(e => e.Status == StatusEnvio.Enviado)
            .Where(e => !StatusCampanhas.TryGetValue(e.CampanhaId, out var status) || status is StatusCampanha.EmAndamento or StatusCampanha.Pausada)
            .ToList();

        return Task.FromResult<IReadOnlyList<Envio>>(resultado);
    }
}
