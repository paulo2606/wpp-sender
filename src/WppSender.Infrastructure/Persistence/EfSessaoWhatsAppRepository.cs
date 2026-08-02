using Microsoft.EntityFrameworkCore;
using WppSender.Domain;

namespace WppSender.Infrastructure.Persistence;

public class EfSessaoWhatsAppRepository : ISessaoWhatsAppRepository
{
    private readonly WppSenderDbContext _db;

    public EfSessaoWhatsAppRepository(WppSenderDbContext db)
    {
        _db = db;
    }

    public async Task<SessaoWhatsApp> ObterAsync()
    {
        return await _db.SessoesWhatsApp.FirstAsync();
    }

    public async Task AtualizarAsync(SessaoWhatsApp sessao)
    {
        if (_db.Entry(sessao).State == EntityState.Detached)
        {
            _db.SessoesWhatsApp.Update(sessao);
        }

        await _db.SaveChangesAsync();
    }
}
