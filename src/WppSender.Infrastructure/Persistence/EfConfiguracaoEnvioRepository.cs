using Microsoft.EntityFrameworkCore;
using WppSender.Domain;

namespace WppSender.Infrastructure.Persistence;

public class EfConfiguracaoEnvioRepository : IConfiguracaoEnvioRepository
{
    private readonly WppSenderDbContext _db;

    public EfConfiguracaoEnvioRepository(WppSenderDbContext db)
    {
        _db = db;
    }

    public async Task<bool> TentarRegistrarEnvioAsync(DateOnly hoje)
    {
        await using var transacao = await _db.Database.BeginTransactionAsync();

        var config = await _db.ConfiguracoesEnvio
            .FromSqlRaw("SELECT * FROM configuracao_envio WHERE id = 1 FOR UPDATE")
            .FirstAsync();

        var conseguiu = config.TentarRegistrarEnvio(hoje);

        await _db.SaveChangesAsync();
        await transacao.CommitAsync();

        return conseguiu;
    }

    public async Task<ConfiguracaoEnvio> ObterAsync()
    {
        return await _db.ConfiguracoesEnvio.AsNoTracking().FirstAsync();
    }
}
