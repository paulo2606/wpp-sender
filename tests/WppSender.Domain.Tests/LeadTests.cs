using WppSender.Domain;
using Xunit;

namespace WppSender.Domain.Tests;

public class LeadTests
{
    [Theory]
    [InlineData("(11) 91234-5678", "11912345678")]
    [InlineData("+55 11 91234-5678", "5511912345678")]
    [InlineData("11912345678", "11912345678")]
    public void DeveNormalizarTelefone_RemovendoTudoQueNaoEDigito(string telefoneDigitado, string telefoneEsperado)
    {
        var lead = new Lead(Guid.NewGuid(), "Fulano", telefoneDigitado, null, null, null);

        Assert.Equal(telefoneEsperado, lead.TelefoneNormalizado);
    }

    [Fact]
    public void DeveEstarAtivo_QuandoRecemCriado()
    {
        var lead = new Lead(Guid.NewGuid(), "Fulano", "11912345678", null, null, null);

        Assert.True(lead.EstaAtivo);
        Assert.Null(lead.DeletadoEm);
    }

    [Fact]
    public void NaoDeveEstarAtivo_AposExcluir()
    {
        var lead = new Lead(Guid.NewGuid(), "Fulano", "11912345678", null, null, null);

        lead.Excluir();

        Assert.False(lead.EstaAtivo);
        Assert.NotNull(lead.DeletadoEm);
    }

    [Fact]
    public void AtualizarDados_DeveNormalizarNovoTelefone()
    {
        var lead = new Lead(Guid.NewGuid(), "Fulano", "11912345678", null, null, null);

        lead.AtualizarDados("Fulano da Silva", "(21) 98888-7777", "novo_insta", null, "site");

        Assert.Equal("Fulano da Silva", lead.Nome);
        Assert.Equal("21988887777", lead.TelefoneNormalizado);
        Assert.Equal("novo_insta", lead.Instagram);
        Assert.Equal("site", lead.Origem);
    }

    [Theory]
    [InlineData(null, "11912345678")]
    [InlineData("", "11912345678")]
    [InlineData("   ", "11912345678")]
    [InlineData("Fulano", null)]
    [InlineData("Fulano", "")]
    [InlineData("Fulano", "   ")]
    public void Construtor_DeveLancarArgumentException_QuandoNomeOuTelefoneVazios(string? nome, string? telefone)
    {
        Assert.Throws<ArgumentException>(() => new Lead(Guid.NewGuid(), nome!, telefone!, null, null, null));
    }

    [Theory]
    [InlineData(null, "11912345678")]
    [InlineData("Fulano", null)]
    [InlineData("", "11912345678")]
    [InlineData("Fulano", "")]
    public void AtualizarDados_DeveLancarArgumentException_QuandoNomeOuTelefoneVazios(string? nome, string? telefone)
    {
        var lead = new Lead(Guid.NewGuid(), "Fulano", "11912345678", null, null, null);

        Assert.Throws<ArgumentException>(() => lead.AtualizarDados(nome!, telefone!, null, null, null));
    }
}
