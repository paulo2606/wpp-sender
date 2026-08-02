namespace WppSender.Domain;

public interface IConfiguracaoEnvioRepository
{
    Task<bool> TentarRegistrarEnvioAsync(DateOnly hoje);
}
