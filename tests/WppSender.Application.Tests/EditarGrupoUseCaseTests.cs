using WppSender.Application.Grupos;
using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using WppSender.Domain;
using Xunit;

namespace WppSender.Application.Tests;

public class EditarGrupoUseCaseTests
{
    private static EditarGrupoUseCase CriarUseCase(FakeGrupoRepository grupoRepositorio, FakeLeadRepository leadRepositorio)
        => new(grupoRepositorio, leadRepositorio, new FakeUnitOfWork());

    [Fact]
    public async Task DeveEditarComSucesso_QuandoGrupoExiste()
    {
        var leadRepositorio = new FakeLeadRepository();
        var repositorio = new FakeGrupoRepository(leadRepositorio);
        var grupo = new Grupo(Guid.NewGuid(), "Nome Antigo", null);
        await repositorio.AdicionarAsync(grupo);
        var useCase = CriarUseCase(repositorio, leadRepositorio);

        var resultado = await useCase.ExecutarAsync(grupo.Id, "Nome Novo", "Descricao Nova");

        Assert.True(resultado.Sucesso);
        var atualizado = await repositorio.BuscarPorIdAsync(grupo.Id);
        Assert.Equal("Nome Novo", atualizado!.Nome);
        Assert.Equal("Descricao Nova", atualizado.Descricao);
    }

    [Fact]
    public async Task DeveRetornarFalha_QuandoGrupoNaoExiste()
    {
        var leadRepositorio = new FakeLeadRepository();
        var repositorio = new FakeGrupoRepository(leadRepositorio);
        var useCase = CriarUseCase(repositorio, leadRepositorio);

        var resultado = await useCase.ExecutarAsync(Guid.NewGuid(), "Nome", null);

        Assert.False(resultado.Sucesso);
        Assert.Equal("Grupo não encontrado", resultado.MensagemErro);
        Assert.Equal(EditarGrupoErro.NaoEncontrado, resultado.Erro);
    }

    [Fact]
    public async Task DeveRetornarFalhaSemErroEspecifico_QuandoNomeInvalido()
    {
        var leadRepositorio = new FakeLeadRepository();
        var repositorio = new FakeGrupoRepository(leadRepositorio);
        var grupo = new Grupo(Guid.NewGuid(), "Nome Antigo", null);
        await repositorio.AdicionarAsync(grupo);
        var useCase = CriarUseCase(repositorio, leadRepositorio);

        var resultado = await useCase.ExecutarAsync(grupo.Id, "", null);

        Assert.False(resultado.Sucesso);
        Assert.Null(resultado.Erro);
    }

    [Fact]
    public async Task DeveAdicionarLeadsAoGrupo_QuandoLeadIdsInformados()
    {
        var leadRepositorio = new FakeLeadRepository();
        var grupoRepositorio = new FakeGrupoRepository(leadRepositorio);
        var criarLeadUseCase = new CriarLeadUseCase(leadRepositorio);
        var lead1 = await criarLeadUseCase.ExecutarAsync("Lead1", "11911111111", null, null, null);
        var lead2 = await criarLeadUseCase.ExecutarAsync("Lead2", "11922222222", null, null, null);
        var grupo = new Grupo(Guid.NewGuid(), "Grupo", null);
        await grupoRepositorio.AdicionarAsync(grupo);
        var useCase = CriarUseCase(grupoRepositorio, leadRepositorio);

        var resultado = await useCase.ExecutarAsync(grupo.Id, "Grupo", null, new[] { lead1.Valor, lead2.Valor });

        Assert.True(resultado.Sucesso);
        var leadAtualizado1 = await leadRepositorio.BuscarPorIdAsync(lead1.Valor);
        var leadAtualizado2 = await leadRepositorio.BuscarPorIdAsync(lead2.Valor);
        Assert.Equal(grupo.Id, leadAtualizado1!.GrupoId);
        Assert.Equal(grupo.Id, leadAtualizado2!.GrupoId);
    }

    [Fact]
    public async Task NaoDeveRemoverLeadsExistentes_QuandoLeadIdsNaoInclui()
    {
        var leadRepositorio = new FakeLeadRepository();
        var grupoRepositorio = new FakeGrupoRepository(leadRepositorio);
        var criarLeadUseCase = new CriarLeadUseCase(leadRepositorio);
        var grupo = new Grupo(Guid.NewGuid(), "Grupo", null);
        await grupoRepositorio.AdicionarAsync(grupo);
        var leadExistente = await criarLeadUseCase.ExecutarAsync("Lead Existente", "11933333333", null, null, null, grupo.Id);
        var useCase = CriarUseCase(grupoRepositorio, leadRepositorio);

        var resultado = await useCase.ExecutarAsync(grupo.Id, "Grupo", null, Array.Empty<Guid>());

        Assert.True(resultado.Sucesso);
        var leadAtualizado = await leadRepositorio.BuscarPorIdAsync(leadExistente.Valor);
        Assert.Equal(grupo.Id, leadAtualizado!.GrupoId);
    }

    [Fact]
    public async Task DeveRetornarFalhaENaoAlterarGrupo_QuandoAlgumLeadNaoExiste()
    {
        var leadRepositorio = new FakeLeadRepository();
        var grupoRepositorio = new FakeGrupoRepository(leadRepositorio);
        var criarLeadUseCase = new CriarLeadUseCase(leadRepositorio);
        var lead1 = await criarLeadUseCase.ExecutarAsync("Lead1", "11911111111", null, null, null);
        var idInexistente = Guid.NewGuid();
        var grupo = new Grupo(Guid.NewGuid(), "Nome Antigo", null);
        await grupoRepositorio.AdicionarAsync(grupo);
        var useCase = CriarUseCase(grupoRepositorio, leadRepositorio);

        var resultado = await useCase.ExecutarAsync(grupo.Id, "Nome Novo", null, new[] { lead1.Valor, idInexistente });

        Assert.False(resultado.Sucesso);
        Assert.Contains(idInexistente.ToString(), resultado.MensagemErro);
        var grupoNaoAlterado = await grupoRepositorio.BuscarPorIdAsync(grupo.Id);
        Assert.Equal("Nome Antigo", grupoNaoAlterado!.Nome);
        var lead1Atualizado = await leadRepositorio.BuscarPorIdAsync(lead1.Valor);
        Assert.Null(lead1Atualizado!.GrupoId);
    }

    [Fact]
    public async Task DeveRetornarFalha_QuandoLeadParaAdicionarJaEstaExcluido()
    {
        var leadRepositorio = new FakeLeadRepository();
        var grupoRepositorio = new FakeGrupoRepository(leadRepositorio);
        var criarLeadUseCase = new CriarLeadUseCase(leadRepositorio);
        var excluirLeadUseCase = new ExcluirLeadUseCase(leadRepositorio);
        var lead1 = await criarLeadUseCase.ExecutarAsync("Lead1", "11911111111", null, null, null);
        await excluirLeadUseCase.ExecutarAsync(lead1.Valor);
        var grupo = new Grupo(Guid.NewGuid(), "Grupo", null);
        await grupoRepositorio.AdicionarAsync(grupo);
        var useCase = CriarUseCase(grupoRepositorio, leadRepositorio);

        var resultado = await useCase.ExecutarAsync(grupo.Id, "Grupo", null, new[] { lead1.Valor });

        Assert.False(resultado.Sucesso);
    }
}
