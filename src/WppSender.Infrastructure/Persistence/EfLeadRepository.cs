using Microsoft.EntityFrameworkCore;
using WppSender.Domain;

namespace WppSender.Infrastructure.Persistence;

public class EfLeadRepository : ILeadRepository
{
    private readonly WppSenderDbContext _db;

    public EfLeadRepository(WppSenderDbContext db)
    {
        _db = db;
    }

    public async Task<Lead?> BuscarPorTelefoneNormalizadoAsync(string telefoneNormalizado)
    {
        return await _db.Leads
            .Include(l => l.Endereco)
            .FirstOrDefaultAsync(l => l.TelefoneNormalizado == telefoneNormalizado && l.DeletadoEm == null);
    }

    public async Task<Lead?> BuscarPorIdAsync(Guid id)
    {
        return await _db.Leads.Include(l => l.Endereco).FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task AdicionarAsync(Lead lead)
    {
        await _db.Leads.AddAsync(lead);
        await _db.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Lead lead)
    {
        if (_db.Entry(lead).State == EntityState.Detached)
        {
            _db.Leads.Update(lead);
        }

        await _db.SaveChangesAsync();
    }

    public async Task<(IReadOnlyList<Lead> Itens, int Total)> ListarAsync(string? busca, int pagina, int tamanhoPagina)
    {
        var query = _db.Leads.Include(l => l.Endereco).AsNoTracking().Where(l => l.DeletadoEm == null);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            query = query.Where(l => l.Nome.Contains(busca) || l.TelefoneNormalizado.Contains(busca));
        }

        var total = await query.CountAsync();
        var itens = await query
            .OrderBy(l => l.Nome).ThenBy(l => l.Id)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return (itens, total);
    }
}
