using WppSender.Application.Dashboard;
using WppSender.Application.Tests.Fakes;
using WppSender.Domain;
using Xunit;

namespace WppSender.Application.Tests;

public class CalcularTaxaEnvioUseCaseTests
{
    [Fact]
    public async Task DeveContarEnviadoFalhouEPendenteAcrossTodasAsCampanhas()
    {
        var envioRepositorio = new FakeEnvioRepository();
        var enviado = new Envio(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        enviado.MarcarComoEnviado(DateTime.UtcNow);
        var falhou = new Envio(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        falhou.MarcarComoFalhou("erro");
        var pendente = new Envio(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        await envioRepositorio.AdicionarVariosAsync(new[] { enviado, falhou, pendente });
        var useCase = new CalcularTaxaEnvioUseCase(envioRepositorio);

        var resultado = await useCase.ExecutarAsync();

        Assert.Equal(1, resultado.Enviado);
        Assert.Equal(1, resultado.Falhou);
        Assert.Equal(1, resultado.Pendente);
    }

    [Fact]
    public async Task DeveRetornarTudoZero_QuandoNaoHaEnvios()
    {
        var envioRepositorio = new FakeEnvioRepository();
        var useCase = new CalcularTaxaEnvioUseCase(envioRepositorio);

        var resultado = await useCase.ExecutarAsync();

        Assert.Equal(0, resultado.Enviado);
        Assert.Equal(0, resultado.Falhou);
        Assert.Equal(0, resultado.Pendente);
    }
}
