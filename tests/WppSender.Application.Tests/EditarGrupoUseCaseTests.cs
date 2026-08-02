using WppSender.Application.Grupos;
using WppSender.Application.Tests.Fakes;
using WppSender.Domain;
using Xunit;

namespace WppSender.Application.Tests;

public class EditarGrupoUseCaseTests
{
    [Fact]
    public async Task DeveEditarComSucesso_QuandoGrupoExiste()
    {
        var repositorio = new FakeGrupoRepository();
        var grupo = new Grupo(Guid.NewGuid(), "Nome Antigo", null);
        await repositorio.AdicionarAsync(grupo);
        var useCase = new EditarGrupoUseCase(repositorio);

        var resultado = await useCase.ExecutarAsync(grupo.Id, "Nome Novo", "Descricao Nova");

        Assert.True(resultado.Sucesso);
        var atualizado = await repositorio.BuscarPorIdAsync(grupo.Id);
        Assert.Equal("Nome Novo", atualizado!.Nome);
        Assert.Equal("Descricao Nova", atualizado.Descricao);
    }

    [Fact]
    public async Task DeveRetornarFalha_QuandoGrupoNaoExiste()
    {
        var repositorio = new FakeGrupoRepository();
        var useCase = new EditarGrupoUseCase(repositorio);

        var resultado = await useCase.ExecutarAsync(Guid.NewGuid(), "Nome", null);

        Assert.False(resultado.Sucesso);
        Assert.Equal("Grupo não encontrado", resultado.MensagemErro);
        Assert.Equal(EditarGrupoErro.NaoEncontrado, resultado.Erro);
    }

    [Fact]
    public async Task DeveRetornarFalhaSemErroEspecifico_QuandoNomeInvalido()
    {
        var repositorio = new FakeGrupoRepository();
        var grupo = new Grupo(Guid.NewGuid(), "Nome Antigo", null);
        await repositorio.AdicionarAsync(grupo);
        var useCase = new EditarGrupoUseCase(repositorio);

        var resultado = await useCase.ExecutarAsync(grupo.Id, "", null);

        Assert.False(resultado.Sucesso);
        Assert.Null(resultado.Erro);
    }
}
