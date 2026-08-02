using WppSender.Application.Campanhas;

namespace WppSender.Application.Tests.Fakes;

public class FakeCampanhaJobScheduler : ICampanhaJobScheduler
{
    public List<(Guid CampanhaId, TimeSpan Atraso)> Agendamentos { get; } = new();

    public Task AgendarProximoEnvioAsync(Guid campanhaId, TimeSpan atraso)
    {
        Agendamentos.Add((campanhaId, atraso));
        return Task.CompletedTask;
    }
}
