using WppSender.Application.Dashboard;
using WppSender.Application.Tests.Fakes;
using WppSender.Domain;
using Xunit;

namespace WppSender.Application.Tests;

public class ObterLimiteDiarioEnvioUseCaseTests
{
    [Fact]
    public async Task DeveCalcularPercentualUsado_QuandoDataReferenciaEHoje()
    {
        var relogio = new FakeRelogio { Agora = new DateTime(2026, 8, 5, 12, 0, 0, DateTimeKind.Utc) };
        var configRepositorio = new FakeConfiguracaoEnvioRepository();
        await configRepositorio.TentarRegistrarEnvioAsync(DateOnly.FromDateTime(relogio.Agora));
        await configRepositorio.TentarRegistrarEnvioAsync(DateOnly.FromDateTime(relogio.Agora));
        var useCase = new ObterLimiteDiarioEnvioUseCase(configRepositorio, relogio);

        var resultado = await useCase.ExecutarAsync();

        Assert.Equal(120, resultado.LimiteDiario);
        Assert.Equal(2, resultado.EnviosRealizadosHoje);
        Assert.Equal(2, resultado.PercentualUsado);
    }

    [Fact]
    public async Task DeveRetornarZero_QuandoContadorEDeUmDiaAnterior()
    {
        var configRepositorio = new FakeConfiguracaoEnvioRepositoryComData(120, 100, new DateOnly(2026, 8, 4));
        var relogio = new FakeRelogio { Agora = new DateTime(2026, 8, 5, 12, 0, 0, DateTimeKind.Utc) };
        var useCase = new ObterLimiteDiarioEnvioUseCase(configRepositorio, relogio);

        var resultado = await useCase.ExecutarAsync();

        Assert.Equal(0, resultado.EnviosRealizadosHoje);
        Assert.Equal(0, resultado.PercentualUsado);
    }
}

internal class FakeConfiguracaoEnvioRepositoryComData : IConfiguracaoEnvioRepository
{
    private readonly ConfiguracaoEnvio _config;

    public FakeConfiguracaoEnvioRepositoryComData(int limiteDiarioEnvios, int enviosRealizadosHoje, DateOnly dataReferencia)
    {
        _config = new ConfiguracaoEnvio(limiteDiarioEnvios, enviosRealizadosHoje, dataReferencia);
    }

    public Task<bool> TentarRegistrarEnvioAsync(DateOnly hoje) => throw new NotImplementedException();

    public Task<ConfiguracaoEnvio> ObterAsync() => Task.FromResult(_config);
}
