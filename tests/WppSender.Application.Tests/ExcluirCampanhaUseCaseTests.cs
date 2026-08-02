using WppSender.Application.Campanhas;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using Xunit;

namespace WppSender.Application.Tests;

public class ExcluirCampanhaUseCaseTests
{
    [Fact]
    public async Task DeveExcluir_QuandoRascunho()
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var grupoId = Guid.NewGuid();
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead1", "11911111111", null, null, null, grupoId);
        var criada = await new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork())
            .ExecutarAsync("Campanha", "Msg", grupoId, null);
        var useCase = new ExcluirCampanhaUseCase(campanhaRepositorio);

        var resultado = await useCase.ExecutarAsync(criada.CampanhaId!.Value);

        Assert.True(resultado.Sucesso);
        Assert.Null(await campanhaRepositorio.BuscarPorIdAsync(criada.CampanhaId.Value));
    }

    [Fact]
    public async Task DeveRetornarFalhaComErroNaoEncontrada_QuandoCampanhaNaoExiste()
    {
        var campanhaRepositorio = new FakeCampanhaRepository();
        var useCase = new ExcluirCampanhaUseCase(campanhaRepositorio);

        var resultado = await useCase.ExecutarAsync(Guid.NewGuid());

        Assert.False(resultado.Sucesso);
        Assert.Equal(ExcluirCampanhaErro.NaoEncontrada, resultado.Erro);
    }

    [Fact]
    public async Task DeveRetornarFalhaComErroNaoPermiteExclusao_QuandoEmAndamento()
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var grupoId = Guid.NewGuid();
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead1", "11911111111", null, null, null, grupoId);
        var criada = await new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork())
            .ExecutarAsync("Campanha", "Msg", grupoId, null);
        var campanha = await campanhaRepositorio.BuscarPorIdAsync(criada.CampanhaId!.Value);
        campanha!.Iniciar();
        await campanhaRepositorio.AtualizarAsync(campanha);
        var useCase = new ExcluirCampanhaUseCase(campanhaRepositorio);

        var resultado = await useCase.ExecutarAsync(criada.CampanhaId.Value);

        Assert.False(resultado.Sucesso);
        Assert.Equal(ExcluirCampanhaErro.NaoPermiteExclusao, resultado.Erro);
    }
}
