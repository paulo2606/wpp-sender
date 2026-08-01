using WppSender.Domain;
using Xunit;

namespace WppSender.Domain.Tests;

public class GrupoTests
{
    [Fact]
    public void DeveCriarComSucesso_QuandoNomeInformado()
    {
        var grupo = new Grupo(Guid.NewGuid(), "Clientes VIP", "Descrição opcional");

        Assert.Equal("Clientes VIP", grupo.Nome);
        Assert.Equal("Descrição opcional", grupo.Descricao);
    }

    [Fact]
    public void DeveLancarExcecao_QuandoNomeVazio()
    {
        Assert.Throws<ArgumentException>(() => new Grupo(Guid.NewGuid(), "", null));
    }

    [Fact]
    public void AtualizarDados_DeveAtualizarNomeEDescricao()
    {
        var grupo = new Grupo(Guid.NewGuid(), "Nome Antigo", "Desc Antiga");

        grupo.AtualizarDados("Nome Novo", "Desc Nova");

        Assert.Equal("Nome Novo", grupo.Nome);
        Assert.Equal("Desc Nova", grupo.Descricao);
    }

    [Fact]
    public void AtualizarDados_DeveLancarExcecao_QuandoNomeVazio()
    {
        var grupo = new Grupo(Guid.NewGuid(), "Nome Antigo", null);

        Assert.Throws<ArgumentException>(() => grupo.AtualizarDados("", null));
    }
}
