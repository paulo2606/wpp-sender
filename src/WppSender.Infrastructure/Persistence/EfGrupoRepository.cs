using Microsoft.EntityFrameworkCore;
using WppSender.Domain;

namespace WppSender.Infrastructure.Persistence;

public class EfGrupoRepository : IGrupoRepository
{
    private readonly WppSenderDbContext _db;

    public EfGrupoRepository(WppSenderDbContext db)
    {
        _db = db;
    }

    public async Task<Grupo?> BuscarPorIdAsync(Guid id)
    {
        return await _db.Grupos.FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task AdicionarAsync(Grupo grupo)
    {
        await _db.Grupos.AddAsync(grupo);
        await _db.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Grupo grupo)
    {
        if (_db.Entry(grupo).State == EntityState.Detached)
        {
            _db.Grupos.Update(grupo);
        }

        await _db.SaveChangesAsync();
    }

    public async Task RemoverAsync(Grupo grupo)
    {
        _db.Grupos.Remove(grupo);
        await _db.SaveChangesAsync();
    }

    public async Task<(IReadOnlyList<(Grupo Grupo, int QuantidadeLeads)> Itens, int Total)> ListarComContagemAsync(int pagina, int tamanhoPagina)
    {
        var total = await _db.Grupos.CountAsync();

        var pagina2 = await _db.Grupos
            .AsNoTracking()
            .OrderBy(g => g.Nome).ThenBy(g => g.Id)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(g => new
            {
                Grupo = g,
                QuantidadeLeads = _db.Leads.Count(l => l.GrupoId == g.Id && l.DeletadoEm == null)
            })
            .ToListAsync();

        var itens = pagina2.Select(x => (x.Grupo, x.QuantidadeLeads)).ToList();

        return (itens, total);
    }

    public async Task<IReadOnlyList<Grupo>> ListarTodosAsync()
    {
        return await _db.Grupos.AsNoTracking().ToListAsync();
    }
}
