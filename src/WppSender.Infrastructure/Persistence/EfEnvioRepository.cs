using Microsoft.EntityFrameworkCore;
using WppSender.Domain;

namespace WppSender.Infrastructure.Persistence;

public class EfEnvioRepository : IEnvioRepository
{
    private readonly WppSenderDbContext _db;

    public EfEnvioRepository(WppSenderDbContext db)
    {
        _db = db;
    }

    public async Task AdicionarVariosAsync(IReadOnlyList<Envio> envios)
    {
        await _db.Envios.AddRangeAsync(envios);
        await _db.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Envio envio)
    {
        if (_db.Entry(envio).State == EntityState.Detached)
        {
            _db.Envios.Update(envio);
        }

        await _db.SaveChangesAsync();
    }

    public async Task<Envio?> BuscarProximoPendenteAsync(Guid campanhaId)
    {
        return await _db.Envios
            .Where(e => e.CampanhaId == campanhaId && e.Status == StatusEnvio.Pendente)
            .OrderBy(e => e.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyDictionary<StatusEnvio, int>> ContarPorStatusAsync(Guid campanhaId)
    {
        var contagens = await _db.Envios
            .Where(e => e.CampanhaId == campanhaId)
            .GroupBy(e => e.Status)
            .Select(g => new { Status = g.Key, Quantidade = g.Count() })
            .ToListAsync();

        return contagens.ToDictionary(c => c.Status, c => c.Quantidade);
    }

    public async Task<IReadOnlyList<Envio>> ListarFalhosAsync(Guid campanhaId)
    {
        return await _db.Envios
            .AsNoTracking()
            .Where(e => e.CampanhaId == campanhaId && e.Status == StatusEnvio.Falhou)
            .OrderBy(e => e.Id)
            .ToListAsync();
    }

    public async Task<IReadOnlyDictionary<StatusEnvio, int>> ContarTodosPorStatusAsync()
    {
        var contagens = await _db.Envios
            .GroupBy(e => e.Status)
            .Select(g => new { Status = g.Key, Quantidade = g.Count() })
            .ToListAsync();

        return contagens.ToDictionary(c => c.Status, c => c.Quantidade);
    }

    public async Task<IReadOnlyList<Envio>> ListarAguardandoConfirmacaoAsync()
    {
        var statusRastreados = new[] { StatusCampanha.EmAndamento, StatusCampanha.Pausada };

        return await (
            from envio in _db.Envios
            join campanha in _db.Campanhas on envio.CampanhaId equals campanha.Id
            where envio.Status == StatusEnvio.Enviado && statusRastreados.Contains(campanha.Status)
            select envio)
            .ToListAsync();
    }
}
