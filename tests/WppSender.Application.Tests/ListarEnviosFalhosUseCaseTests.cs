using WppSender.Application.Campanhas;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using Xunit;

namespace WppSender.Application.Tests;

public class ListarEnviosFalhosUseCaseTests
{
    [Fact]
    public async Task DeveListarApenasFalhos()
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var grupoId = Guid.NewGuid();
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead1", "11911111111", null, null, null, grupoId);
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead2", "11922222222", null, null, null, grupoId);
        var criada = await new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork())
            .ExecutarAsync("Campanha", "Msg", grupoId, null);
        var envio = envioRepositorio.Todos.First(e => e.CampanhaId == criada.Valor);
        envio.MarcarComoFalhou("Erro de teste");
        await envioRepositorio.AtualizarAsync(envio);
        var useCase = new ListarEnviosFalhosUseCase(envioRepositorio, leadRepositorio);

        var resultado = await useCase.ExecutarAsync(criada.Valor);

        Assert.Single(resultado);
        Assert.Equal("Erro de teste", resultado[0].Erro);
    }
}
