using WppSender.Application.Campanhas;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using WppSender.Domain;
using Xunit;

namespace WppSender.Application.Tests;

public class PausarCampanhaUseCaseTests
{
    [Fact]
    public async Task DevePausar_QuandoEmAndamento()
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var grupoId = Guid.NewGuid();
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead1", "11911111111", null, null, null, grupoId);
        var criada = await new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork())
            .ExecutarAsync("Campanha", "Msg", grupoId, null);
        var campanha = await campanhaRepositorio.BuscarPorIdAsync(criada.Valor);
        campanha!.Iniciar();
        await campanhaRepositorio.AtualizarAsync(campanha);
        var useCase = new PausarCampanhaUseCase(campanhaRepositorio);

        var resultado = await useCase.ExecutarAsync(criada.Valor);

        Assert.True(resultado.Sucesso);
        var recarregada = await campanhaRepositorio.BuscarPorIdAsync(criada.Valor);
        Assert.Equal(StatusCampanha.Pausada, recarregada!.Status);
    }

    [Fact]
    public async Task DeveRetornarFalhaComErroStatusInvalido_QuandoRascunho()
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var grupoId = Guid.NewGuid();
        await new CriarLeadUseCase(leadRepositorio).ExecutarAsync("Lead1", "11911111111", null, null, null, grupoId);
        var criada = await new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork())
            .ExecutarAsync("Campanha", "Msg", grupoId, null);
        var useCase = new PausarCampanhaUseCase(campanhaRepositorio);

        var resultado = await useCase.ExecutarAsync(criada.Valor);

        Assert.False(resultado.Sucesso);
        Assert.Equal(PausarCampanhaErro.StatusInvalido, resultado.Erro);
    }

    [Fact]
    public async Task DeveRetornarFalhaComErroNaoEncontrada_QuandoCampanhaNaoExiste()
    {
        var useCase = new PausarCampanhaUseCase(new FakeCampanhaRepository());

        var resultado = await useCase.ExecutarAsync(Guid.NewGuid());

        Assert.False(resultado.Sucesso);
        Assert.Equal(PausarCampanhaErro.NaoEncontrada, resultado.Erro);
    }
}
