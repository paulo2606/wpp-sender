using WppSender.Application.Campanhas;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using WppSender.Domain;
using Xunit;

namespace WppSender.Application.Tests;

public class ListarCampanhasUseCaseTests
{
    [Fact]
    public async Task DeveFiltrarPorStatus_QuandoInformado()
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var grupoId = Guid.NewGuid();
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead1", "11911111111", null, null, null, grupoId);
        var criarUseCase = new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork());
        await criarUseCase.ExecutarAsync("Rascunho", "Msg", grupoId, null);
        await criarUseCase.ExecutarAsync("Agendada", "Msg", grupoId, DateTime.UtcNow.AddDays(1));
        var useCase = new ListarCampanhasUseCase(campanhaRepositorio);

        var resultado = await useCase.ExecutarAsync(StatusCampanha.Rascunho, 1, 10);

        Assert.Equal(1, resultado.Total);
        Assert.Equal("Rascunho", resultado.Itens[0].Nome);
    }

    [Fact]
    public async Task DeveClamparPaginaETamanhoPagina_QuandoValoresInvalidos()
    {
        var campanhaRepositorio = new FakeCampanhaRepository();
        var useCase = new ListarCampanhasUseCase(campanhaRepositorio);

        var resultado = await useCase.ExecutarAsync(null, 0, 1000);

        Assert.Equal(1, resultado.Pagina);
        Assert.Equal(100, resultado.TamanhoPagina);
    }
}
