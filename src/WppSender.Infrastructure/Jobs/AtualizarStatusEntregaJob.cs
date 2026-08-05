using WppSender.Application.Campanhas;

namespace WppSender.Infrastructure.Jobs;

public class AtualizarStatusEntregaJob
{
    private readonly AtualizarStatusEntregaUseCase _useCase;

    public AtualizarStatusEntregaJob(AtualizarStatusEntregaUseCase useCase)
    {
        _useCase = useCase;
    }

    public Task ExecutarAsync() => _useCase.ExecutarAsync();
}
