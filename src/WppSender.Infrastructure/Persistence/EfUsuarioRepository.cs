using Microsoft.EntityFrameworkCore;
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
        await _db.SaveChangesAsync();
    }
}
