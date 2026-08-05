using WppSender.Application.Campanhas;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using WppSender.Domain;
using Xunit;

namespace WppSender.Application.Tests;

public class CancelarCampanhaUseCaseTests
{
    private static async Task<(FakeCampanhaRepository CampanhaRepositorio, FakeEnvioRepository EnvioRepositorio, Campanha Campanha)> CriarCampanhaPausadaComEnviosAsync(int quantidadeLeads)
    {
        var leadRepositorio = new FakeLeadRepository();
        var campanhaRepositorio = new FakeCampanhaRepository();
        var envioRepositorio = new FakeEnvioRepository();
        var grupoId = Guid.NewGuid();
        for (var i = 0; i < quantidadeLeads; i++)
        {
            await new CriarLeadUseCase(leadRepositorio).ExecutarAsync($"Lead{i}", $"1191111111{i}", null, null, null, grupoId);
        }

        var criada = await new CriarCampanhaUseCase(campanhaRepositorio, envioRepositorio, leadRepositorio, new FakeUnitOfWork())
            .ExecutarAsync("Campanha", "Msg", grupoId, null);
        var campanha = await campanhaRepositorio.BuscarPorIdAsync(criada.CampanhaId!.Value);
        campanha!.Iniciar();
        campanha.Pausar();
        await campanhaRepositorio.AtualizarAsync(campanha);

        return (campanhaRepositorio, envioRepositorio, campanha);
    }

    [Fact]
    public async Task DeveCancelar_QuandoPausada()
    {
        var (campanhaRepositorio, envioRepositorio, campanha) = await CriarCampanhaPausadaComEnviosAsync(1);
        var useCase = new CancelarCampanhaUseCase(campanhaRepositorio, envioRepositorio);

        var resultado = await useCase.ExecutarAsync(campanha.Id);

        Assert.True(resultado.Sucesso);
        var recarregada = await campanhaRepositorio.BuscarPorIdAsync(campanha.Id);
        Assert.Equal(StatusCampanha.Cancelada, recarregada!.Status);
    }

    [Fact]
    public async Task DeveMarcarPendentesComoFalhou_AoCancelar()
    {
        var (campanhaRepositorio, envioRepositorio, campanha) = await CriarCampanhaPausadaComEnviosAsync(2);
        var useCase = new CancelarCampanhaUseCase(campanhaRepositorio, envioRepositorio);

        await useCase.ExecutarAsync(campanha.Id);

        var contagens = await envioRepositorio.ContarPorStatusAsync(campanha.Id);
        Assert.Equal(2, contagens[StatusEnvio.Falhou]);
        Assert.False(contagens.ContainsKey(StatusEnvio.Pendente));
    }

    [Fact]
    public async Task NaoDeveTocarEnvioJaEnviado_AoCancelar()
    {
        var (campanhaRepositorio, envioRepositorio, campanha) = await CriarCampanhaPausadaComEnviosAsync(2);
        var jaEnviado = envioRepositorio.Todos.First();
        jaEnviado.MarcarComoEnviado(DateTime.UtcNow, "wamid.1");
        await envioRepositorio.AtualizarAsync(jaEnviado);
        var useCase = new CancelarCampanhaUseCase(campanhaRepositorio, envioRepositorio);

        await useCase.ExecutarAsync(campanha.Id);

        var contagens = await envioRepositorio.ContarPorStatusAsync(campanha.Id);
        Assert.Equal(1, contagens[StatusEnvio.Enviado]);
        Assert.Equal(1, contagens[StatusEnvio.Falhou]);
    }

    [Fact]
    public async Task DeveRetornarFalhaComErroStatusInvalido_QuandoEmAndamento()
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
        var useCase = new CancelarCampanhaUseCase(campanhaRepositorio, envioRepositorio);

        var resultado = await useCase.ExecutarAsync(campanha.Id);

        Assert.False(resultado.Sucesso);
        Assert.Equal(CancelarCampanhaErro.StatusInvalido, resultado.Erro);
    }

    [Fact]
    public async Task DeveRetornarFalhaComErroNaoEncontrada_QuandoCampanhaNaoExiste()
    {
        var useCase = new CancelarCampanhaUseCase(new FakeCampanhaRepository(), new FakeEnvioRepository());

        var resultado = await useCase.ExecutarAsync(Guid.NewGuid());

        Assert.False(resultado.Sucesso);
        Assert.Equal(CancelarCampanhaErro.NaoEncontrada, resultado.Erro);
    }
}
