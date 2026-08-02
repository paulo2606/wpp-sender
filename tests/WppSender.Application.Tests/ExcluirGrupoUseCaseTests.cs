using WppSender.Application.Grupos;
using WppSender.Application.Tests.Fakes;
using WppSender.Domain;
using Xunit;

namespace WppSender.Application.Tests;

public class ExcluirGrupoUseCaseTests
{
    [Fact]
    public async Task DeveRemover_QuandoGrupoExiste()
    {
        var repositorio = new FakeGrupoRepository();
        var grupo = new Grupo(Guid.NewGuid(), "Grupo", null);
        await repositorio.AdicionarAsync(grupo);
        var useCase = new ExcluirGrupoUseCase(repositorio);

        var resultado = await useCase.ExecutarAsync(grupo.Id);

        Assert.True(resultado.Sucesso);
        var encontrado = await repositorio.BuscarPorIdAsync(grupo.Id);
        Assert.Null(encontrado);
    }

    [Fact]
    public async Task DeveRetornarFalha_QuandoGrupoNaoExiste()
    {
        var repositorio = new FakeGrupoRepository();
        var useCase = new ExcluirGrupoUseCase(repositorio);

        var resultado = await useCase.ExecutarAsync(Guid.NewGuid());

        Assert.False(resultado.Sucesso);
        Assert.Equal("Grupo não encontrado", resultado.MensagemErro);
    }
}
