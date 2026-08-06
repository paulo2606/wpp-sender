using WppSender.Application.Campanhas;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using Xunit;

namespace WppSender.Application.Tests;

public class ObterCampanhaUseCaseTests
{
    [Fact]
    public async Task DeveRetornarCampanhaComProgresso()
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var grupoId = Guid.NewGuid();
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead1", "11911111111", null, null, null, grupoId);
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead2", "11922222222", null, null, null, grupoId);
        var criada = await new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork())
            .ExecutarAsync("Campanha", "Msg", grupoId, null);
        var useCase = new ObterCampanhaUseCase(campanhaRepositorio, envioRepositorio);

        var resultado = await useCase.ExecutarAsync(criada.Valor);

        Assert.NotNull(resultado);
        Assert.Equal("Campanha", resultado!.Campanha.Nome);
        Assert.Equal(2, resultado.Progresso.Pendente);
        Assert.Equal(0, resultado.Progresso.Enviado);
        Assert.Equal(0, resultado.Progresso.Falhou);
    }

    [Fact]
    public async Task DeveRetornarNulo_QuandoCampanhaNaoExiste()
    {
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var useCase = new ObterCampanhaUseCase(campanhaRepositorio, envioRepositorio);

        var resultado = await useCase.ExecutarAsync(Guid.NewGuid());

        Assert.Null(resultado);
    }
}
