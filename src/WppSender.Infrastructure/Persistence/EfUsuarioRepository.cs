using Microsoft.EntityFrameworkCore;
using Npgsql;
using WppSender.Domain;

namespace WppSender.Infrastructure.Persistence;

public class EfUsuarioRepository : IUsuarioRepository
{
    private readonly WppSenderDbContext _db;

    public EfUsuarioRepository(WppSenderDbContext db)
    {
        _db = db;
    }

    public async Task<Usuario?> BuscarPorEmailAsync(string email)
    {
        return await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> ExisteAlgumAsync()
    {
        return await _db.Usuarios.AnyAsync();
    }

    public async Task AdicionarAsync(Usuario usuario)
    {
        await _db.Usuarios.AddAsync(usuario);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" })
        {
            throw new InvalidOperationException("Já existe um usuário cadastrado", ex);
        }
    }

    public async Task<bool> AdicionarSeForOPrimeiroAsync(Usuario usuario)
    {
        await using var transacao = await _db.Database.BeginTransactionAsync();
        await _db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(727001)");

        if (await _db.Usuarios.AnyAsync())
        {
            return false;
        }

        await _db.Usuarios.AddAsync(usuario);
        await _db.SaveChangesAsync();
        await transacao.CommitAsync();
        return true;
    }
}
