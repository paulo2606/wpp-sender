using Hangfire;
using WppSender.Application.Campanhas;

namespace WppSender.Infrastructure.Jobs;

public class CampanhaSendJob
{
    private readonly ProcessarProximoEnvioUseCase _useCase;

    public CampanhaSendJob(ProcessarProximoEnvioUseCase useCase)
    {
        _useCase = useCase;
    }

    public async Task ExecutarAsync(Guid campanhaId)
    {

        using var lockDistribuido = JobStorage.Current.GetConnection()
            .AcquireDistributedLock($"campanha-envio-{campanhaId}", TimeSpan.FromSeconds(30));

        await _useCase.ExecutarAsync(campanhaId);
    }
}
