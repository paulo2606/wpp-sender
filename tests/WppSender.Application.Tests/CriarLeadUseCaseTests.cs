using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using Xunit;

namespace WppSender.Application.Tests;

public class CriarLeadUseCaseTests
{
    [Fact]
    public async Task DeveCriarComSucesso_QuandoTelefoneNaoExisteAindaEntreAtivos()
    {
        var repositorio = new FakeLeadRepository();
        var useCase = new CriarLeadUseCase(repositorio);

        var resultado = await useCase.ExecutarAsync("Fulano", "11912345678", "fulano_insta", null, "site");

        Assert.True(resultado.Sucesso);
        var criado = await repositorio.BuscarPorTelefoneNormalizadoAsync("11912345678");
        Assert.NotNull(criado);
        Assert.Equal("Fulano", criado!.Nome);
    }

    [Fact]
    public async Task DeveCriarComEndereco_QuandoEnderecoInformado()
    {
        var repositorio = new FakeLeadRepository();
        var useCase = new CriarLeadUseCase(repositorio);
        var endereco = new EnderecoInput("Rua A", "100", null, "Centro", "São Paulo", "SP", "01000-000");

        var resultado = await useCase.ExecutarAsync("Fulano", "11912345678", null, endereco, null);

        Assert.True(resultado.Sucesso);
        var criado = await repositorio.BuscarPorTelefoneNormalizadoAsync("11912345678");
        Assert.NotNull(criado!.Endereco);
        Assert.Equal("Rua A", criado.Endereco!.Rua);
        Assert.Equal("São Paulo", criado.Endereco.Cidade);
    }

    [Fact]
    public async Task DeveRetornarFalha_QuandoTelefoneJaExisteEntreAtivos()
    {
        var repositorio = new FakeLeadRepository();
        var useCase = new CriarLeadUseCase(repositorio);
        await useCase.ExecutarAsync("Primeiro", "11912345678", null, null, null);

        var resultado = await useCase.ExecutarAsync("Segundo", "(11) 91234-5678", null, null, null);

        Assert.False(resultado.Sucesso);
        Assert.Equal("Telefone já cadastrado", resultado.MensagemErro);
    }

    [Theory]
    [InlineData(null, "11912345678")]
    [InlineData("", "11912345678")]
    [InlineData("Fulano", null)]
    [InlineData("Fulano", "")]
    public async Task DeveRetornarFalha_QuandoNomeOuTelefoneVazios(string? nome, string? telefone)
    {
        var repositorio = new FakeLeadRepository();
        var useCase = new CriarLeadUseCase(repositorio);

        var resultado = await useCase.ExecutarAsync(nome!, telefone!, null, null, null);

        Assert.False(resultado.Sucesso);
        Assert.NotNull(resultado.MensagemErro);
    }

    [Fact]
    public async Task DevePermitirCriar_QuandoTelefoneEraDeUmLeadJaExcluido()
    {
        var repositorio = new FakeLeadRepository();
        var useCase = new CriarLeadUseCase(repositorio);
        var primeiroResultado = await useCase.ExecutarAsync("Primeiro", "11912345678", null, null, null);
        var primeiroLead = await repositorio.BuscarPorIdAsync(primeiroResultado.Valor);
        primeiroLead!.Excluir();
        await repositorio.AtualizarAsync(primeiroLead);

        var resultado = await useCase.ExecutarAsync("Segundo", "11912345678", null, null, null);

        Assert.True(resultado.Sucesso);
    }

    [Fact]
    public async Task DeveCriarComGrupoId_QuandoInformado()
    {
        var repositorio = new FakeLeadRepository();
        var useCase = new CriarLeadUseCase(repositorio);
        var grupoId = Guid.NewGuid();

        var resultado = await useCase.ExecutarAsync("Fulano", "11912345678", null, null, null, grupoId);

        Assert.True(resultado.Sucesso);
        var criado = await repositorio.BuscarPorIdAsync(resultado.Valor);
        Assert.Equal(grupoId, criado!.GrupoId);
    }
}
