using WppSender.Domain;

namespace WppSender.Infrastructure.Persistence;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly WppSenderDbContext _db;

    public EfUnitOfWork(WppSenderDbContext db)
    {
        _db = db;
    }

    public async Task ExecutarTransacaoAsync(Func<Task> operacao)
    {
        await using var transacao = await _db.Database.BeginTransactionAsync();
        try
        {
            await operacao();
            await transacao.CommitAsync();
        }
        catch
        {
            await transacao.RollbackAsync();
            throw;
        }
    }
}
