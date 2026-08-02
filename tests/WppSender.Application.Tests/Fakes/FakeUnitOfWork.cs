using WppSender.Domain;

namespace WppSender.Application.Tests.Fakes;

public class FakeUnitOfWork : IUnitOfWork
{
    public async Task ExecutarTransacaoAsync(Func<Task> operacao)
    {
        await operacao();
    }
}
