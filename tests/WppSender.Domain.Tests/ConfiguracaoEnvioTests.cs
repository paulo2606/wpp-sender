using WppSender.Domain;
using Xunit;

namespace WppSender.Domain.Tests;

public class ConfiguracaoEnvioTests
{
    [Fact]
    public void TentarRegistrarEnvio_DeveRetornarTrueEIncrementar_QuandoAbaixoDoLimite()
    {
        var config = new ConfiguracaoEnvio(limiteDiarioEnvios: 5, enviosRealizadosHoje: 2, dataReferencia: new DateOnly(2026, 8, 2));

        var resultado = config.TentarRegistrarEnvio(new DateOnly(2026, 8, 2));

        Assert.True(resultado);
        Assert.Equal(3, config.EnviosRealizadosHoje);
    }

    [Fact]
    public void TentarRegistrarEnvio_DeveRetornarFalse_QuandoLimiteAtingido()
    {
        var config = new ConfiguracaoEnvio(limiteDiarioEnvios: 3, enviosRealizadosHoje: 3, dataReferencia: new DateOnly(2026, 8, 2));

        var resultado = config.TentarRegistrarEnvio(new DateOnly(2026, 8, 2));

        Assert.False(resultado);
        Assert.Equal(3, config.EnviosRealizadosHoje);
    }

    [Fact]
    public void TentarRegistrarEnvio_DeveResetarContador_QuandoVirouODia()
    {
        var config = new ConfiguracaoEnvio(limiteDiarioEnvios: 3, enviosRealizadosHoje: 3, dataReferencia: new DateOnly(2026, 8, 1));

        var resultado = config.TentarRegistrarEnvio(new DateOnly(2026, 8, 2));

        Assert.True(resultado);
        Assert.Equal(1, config.EnviosRealizadosHoje);
        Assert.Equal(new DateOnly(2026, 8, 2), config.DataReferencia);
    }
}
