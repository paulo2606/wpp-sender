namespace WppSender.Domain;

public interface IUnitOfWork
{
    Task ExecutarTransacaoAsync(Func<Task> operacao);
}
