using WppSender.Application.Campanhas;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using Xunit;

namespace WppSender.Application.Tests;

public class EditarCampanhaUseCaseTests
{
    private static async Task<(FakeCampanhaRepository CampanhaRepositorio, FakeEnvioRepository EnvioRepositorio, FakeLeadRepository LeadRepositorio, Guid CampanhaId)> CriarCenarioComCampanhaAsync()
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var criarLeadUseCase = new CriarLeadUseCase(leadRepositorio);
        var grupoId = Guid.NewGuid();
        await criarLeadUseCase.ExecutarAsync("Lead1", "11911111111", null, null, null, grupoId);
        var criarCampanhaUseCase = new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork());
        var criada = await criarCampanhaUseCase.ExecutarAsync("Nome Antigo", "Msg Antiga", grupoId, agendadoPara: null);

        return (campanhaRepositorio, envioRepositorio, leadRepositorio, criada.Valor);
    }

    [Fact]
    public async Task DeveEditarComSucesso_QuandoRascunho()
    {
        var (campanhaRepositorio, _, _, campanhaId) = await CriarCenarioComCampanhaAsync();
        var useCase = new EditarCampanhaUseCase(campanhaRepositorio);

        var resultado = await useCase.ExecutarAsync(campanhaId, "Nome Novo", "Msg Nova", null, 40, 100);

        Assert.True(resultado.Sucesso);
        var campanha = await campanhaRepositorio.BuscarPorIdAsync(campanhaId);
        Assert.Equal("Nome Novo", campanha!.Nome);
    }

    [Fact]
    public async Task DeveRetornarFalhaComErroNaoEncontrado_QuandoCampanhaNaoExiste()
    {
        var campanhaRepositorio = new FakeCampanhaRepository();
        var useCase = new EditarCampanhaUseCase(campanhaRepositorio);

        var resultado = await useCase.ExecutarAsync(Guid.NewGuid(), "Nome", "Msg", null, 30, 90);

        Assert.False(resultado.Sucesso);
        Assert.Equal(EditarCampanhaErro.NaoEncontrada, resultado.Erro);
    }

    [Fact]
    public async Task DeveRetornarFalhaComErroNaoPermiteEdicao_QuandoEmAndamento()
    {
        var (campanhaRepositorio, _, _, campanhaId) = await CriarCenarioComCampanhaAsync();
        var campanha = await campanhaRepositorio.BuscarPorIdAsync(campanhaId);
        campanha!.Iniciar();
        await campanhaRepositorio.AtualizarAsync(campanha);
        var useCase = new EditarCampanhaUseCase(campanhaRepositorio);

        var resultado = await useCase.ExecutarAsync(campanhaId, "Nome", "Msg", null, 30, 90);

        Assert.False(resultado.Sucesso);
        Assert.Equal(EditarCampanhaErro.NaoPermiteEdicao, resultado.Erro);
    }
}
