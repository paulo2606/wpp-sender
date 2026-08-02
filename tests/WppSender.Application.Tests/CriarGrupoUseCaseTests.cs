using WppSender.Application.Grupos;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using Xunit;

namespace WppSender.Application.Tests;

public class CriarGrupoUseCaseTests
{
    [Fact]
    public async Task DeveCriarEVincularLeads_QuandoTodosValidos()
    {
        var leadRepositorio = new FakeLeadRepository();
        var grupoRepositorio = new FakeGrupoRepository(leadRepositorio);
        var criarLeadUseCase = new CriarLeadUseCase(leadRepositorio);
        var lead1 = await criarLeadUseCase.ExecutarAsync("Lead1", "11911111111", null, null, null);
        var lead2 = await criarLeadUseCase.ExecutarAsync("Lead2", "11922222222", null, null, null);
        var useCase = new CriarGrupoUseCase(grupoRepositorio, leadRepositorio, new FakeUnitOfWork());

        var resultado = await useCase.ExecutarAsync("Clientes VIP", "Descricao", new[] { lead1.LeadId!.Value, lead2.LeadId!.Value });

        Assert.True(resultado.Sucesso);
        var leadAtualizado1 = await leadRepositorio.BuscarPorIdAsync(lead1.LeadId.Value);
        var leadAtualizado2 = await leadRepositorio.BuscarPorIdAsync(lead2.LeadId.Value);
        Assert.Equal(resultado.GrupoId, leadAtualizado1!.GrupoId);
        Assert.Equal(resultado.GrupoId, leadAtualizado2!.GrupoId);
    }

    [Fact]
    public async Task DeveRetornarFalha_QuandoListaDeLeadsVazia()
    {
        var leadRepositorio = new FakeLeadRepository();
        var grupoRepositorio = new FakeGrupoRepository(leadRepositorio);
        var useCase = new CriarGrupoUseCase(grupoRepositorio, leadRepositorio, new FakeUnitOfWork());

        var resultado = await useCase.ExecutarAsync("Grupo", null, Array.Empty<Guid>());

        Assert.False(resultado.Sucesso);
        Assert.Equal("É necessário informar ao menos um lead", resultado.MensagemErro);
    }

    [Fact]
    public async Task DeveRetornarFalhaENaoCriarGrupo_QuandoAlgumLeadNaoExiste()
    {
        var leadRepositorio = new FakeLeadRepository();
        var grupoRepositorio = new FakeGrupoRepository(leadRepositorio);
        var criarLeadUseCase = new CriarLeadUseCase(leadRepositorio);
        var lead1 = await criarLeadUseCase.ExecutarAsync("Lead1", "11911111111", null, null, null);
        var idInexistente = Guid.NewGuid();
        var useCase = new CriarGrupoUseCase(grupoRepositorio, leadRepositorio, new FakeUnitOfWork());

        var resultado = await useCase.ExecutarAsync("Grupo", null, new[] { lead1.LeadId!.Value, idInexistente });

        Assert.False(resultado.Sucesso);
        Assert.Contains(idInexistente.ToString(), resultado.MensagemErro);
        var (_, total) = await grupoRepositorio.ListarComContagemAsync(1, 10);
        Assert.Equal(0, total);
    }

    [Fact]
    public async Task DeveRetornarFalha_QuandoLeadJaEstaExcluido()
    {
        var leadRepositorio = new FakeLeadRepository();
        var grupoRepositorio = new FakeGrupoRepository(leadRepositorio);
        var criarLeadUseCase = new CriarLeadUseCase(leadRepositorio);
        var excluirLeadUseCase = new ExcluirLeadUseCase(leadRepositorio);
        var lead1 = await criarLeadUseCase.ExecutarAsync("Lead1", "11911111111", null, null, null);
        await excluirLeadUseCase.ExecutarAsync(lead1.LeadId!.Value);
        var useCase = new CriarGrupoUseCase(grupoRepositorio, leadRepositorio, new FakeUnitOfWork());

        var resultado = await useCase.ExecutarAsync("Grupo", null, new[] { lead1.LeadId.Value });

        Assert.False(resultado.Sucesso);
    }
}
