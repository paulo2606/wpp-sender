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
        // Lock distribuído escopado por campanhaId: evita que duas cadeias da MESMA
        // campanha rodem em paralelo (ex.: um resume duplicado), sem bloquear campanhas
        // diferentes entre si (ao contrário do [DisableConcurrentExecution] genérico).
        // Observação: Hangfire.Core (versão estável mais recente, 1.8.x) só expõe a
        // variante síncrona de AcquireDistributedLock — não há AcquireDistributedLockAsync
        // no storage async (esse recurso é do Hangfire 2.0, ainda não lançado como estável).
        using var lockDistribuido = JobStorage.Current.GetConnection()
            .AcquireDistributedLock($"campanha-envio-{campanhaId}", TimeSpan.FromSeconds(30));

        await _useCase.ExecutarAsync(campanhaId);
    }
}
