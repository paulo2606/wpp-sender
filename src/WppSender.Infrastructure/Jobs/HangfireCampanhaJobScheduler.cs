using Hangfire;
using WppSender.Application.Campanhas;

namespace WppSender.Infrastructure.Jobs;

public class HangfireCampanhaJobScheduler : ICampanhaJobScheduler
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireCampanhaJobScheduler(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public Task AgendarProximoEnvioAsync(Guid campanhaId, TimeSpan atraso)
    {
        _backgroundJobClient.Schedule<CampanhaSendJob>(job => job.ExecutarAsync(campanhaId), atraso);
        return Task.CompletedTask;
    }
}
