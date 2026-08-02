using Microsoft.EntityFrameworkCore;
using WppSender.Domain;

namespace WppSender.Infrastructure.Persistence;

public class EfCampanhaRepository : ICampanhaRepository
{
    private readonly WppSenderDbContext _db;

    public EfCampanhaRepository(WppSenderDbContext db)
    {
        _db = db;
    }

    public async Task<Campanha?> BuscarPorIdAsync(Guid id)
    {
        return await _db.Campanhas.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AdicionarAsync(Campanha campanha)
    {
        await _db.Campanhas.AddAsync(campanha);
        await _db.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Campanha campanha)
    {
        if (_db.Entry(campanha).State == EntityState.Detached)
        {
            _db.Campanhas.Update(campanha);
        }

        await _db.SaveChangesAsync();
    }

    public async Task RemoverAsync(Campanha campanha)
    {
        _db.Campanhas.Remove(campanha);
        await _db.SaveChangesAsync();
    }

    public async Task<(IReadOnlyList<Campanha> Itens, int Total)> ListarAsync(StatusCampanha? status, int pagina, int tamanhoPagina)
    {
        var query = _db.Campanhas.AsNoTracking().AsQueryable();

        if (status is not null)
        {
            query = query.Where(c => c.Status == status);
        }

        var total = await query.CountAsync();
        var itens = await query
            .OrderBy(c => c.Nome).ThenBy(c => c.Id)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return (itens, total);
    }

    public async Task<IReadOnlyList<Campanha>> ListarAgendadasParaIniciarAsync(DateTime agora)
    {
        return await _db.Campanhas
            .Where(c => c.Status == StatusCampanha.Agendada && c.AgendadoPara != null && c.AgendadoPara <= agora)
            .ToListAsync();
    }

    public async Task<IReadOnlyDictionary<StatusCampanha, int>> ContarPorStatusAsync()
    {
        var contagens = await _db.Campanhas
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Quantidade = g.Count() })
            .ToListAsync();

        return contagens.ToDictionary(c => c.Status, c => c.Quantidade);
    }

    public async Task<Campanha?> ObterProximaAgendadaAsync()
    {
        return await _db.Campanhas
            .Where(c => c.Status == StatusCampanha.Agendada && c.AgendadoPara != null)
            .OrderBy(c => c.AgendadoPara)
            .FirstOrDefaultAsync();
    }
}
