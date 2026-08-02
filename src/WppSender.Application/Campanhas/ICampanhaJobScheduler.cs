namespace WppSender.Application.Campanhas;

public interface ICampanhaJobScheduler
{
    Task AgendarProximoEnvioAsync(Guid campanhaId, TimeSpan atraso);
}
