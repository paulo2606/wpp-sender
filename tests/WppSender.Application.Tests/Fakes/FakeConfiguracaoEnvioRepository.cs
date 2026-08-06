using WppSender.Domain;

namespace WppSender.Application.Tests.Fakes;

public class FakeConfiguracaoEnvioRepository : IConfiguracaoEnvioRepository
{
    private ConfiguracaoEnvio _config;

    public FakeConfiguracaoEnvioRepository(int limiteDiarioEnvios = 120)
    {
        _config = new ConfiguracaoEnvio(limiteDiarioEnvios, 0, new DateOnly(2000, 1, 1));
    }

    public Task<bool> TentarRegistrarEnvioAsync(DateOnly hoje)
    {
        return Task.FromResult(_config.TentarRegistrarEnvio(hoje));
    }

    public Task<ConfiguracaoEnvio> ObterAsync()
    {
        return Task.FromResult(_config);
    }
}
