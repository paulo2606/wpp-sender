using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using Xunit;

namespace WppSender.Application.Tests;

public class EditarLeadUseCaseTests
{
    [Fact]
    public async Task DeveEditarComSucesso_QuandoLeadExiste()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var editarUseCase = new EditarLeadUseCase(repositorio);
        var criado = await criarUseCase.ExecutarAsync("Fulano", "11911111111", null, null, null);

        var resultado = await editarUseCase.ExecutarAsync(criado.Valor, "Fulano Editado", "11922222222", "insta_novo", null, "origem_nova");

        Assert.True(resultado.Sucesso);
        var atualizado = await repositorio.BuscarPorIdAsync(criado.Valor);
        Assert.Equal("Fulano Editado", atualizado!.Nome);
        Assert.Equal("11922222222", atualizado.TelefoneNormalizado);
    }

    [Fact]
    public async Task DeveRetornarFalha_QuandoLeadNaoExiste()
    {
        var repositorio = new FakeLeadRepository();
        var editarUseCase = new EditarLeadUseCase(repositorio);

        var resultado = await editarUseCase.ExecutarAsync(Guid.NewGuid(), "Nome", "11911111111", null, null, null);

        Assert.False(resultado.Sucesso);
        Assert.Equal("Lead não encontrado", resultado.MensagemErro);
        Assert.Equal(EditarLeadErro.NaoEncontrado, resultado.Erro);
    }

    [Theory]
    [InlineData(null, "11911111111")]
    [InlineData("", "11911111111")]
    [InlineData("Nome", null)]
    [InlineData("Nome", "")]
    public async Task DeveRetornarFalha_QuandoNomeOuTelefoneVazios(string? nome, string? telefone)
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var editarUseCase = new EditarLeadUseCase(repositorio);
        var criado = await criarUseCase.ExecutarAsync("Fulano", "11911111111", null, null, null);

        var resultado = await editarUseCase.ExecutarAsync(criado.Valor, nome!, telefone!, null, null, null);

        Assert.False(resultado.Sucesso);
        Assert.NotNull(resultado.MensagemErro);
        Assert.Null(resultado.Erro);
    }

    [Fact]
    public async Task DeveRetornarFalha_QuandoNovoTelefoneJaPertenceAOutroLeadAtivo()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var editarUseCase = new EditarLeadUseCase(repositorio);
        var lead1 = await criarUseCase.ExecutarAsync("Lead1", "11911111111", null, null, null);
        var lead2 = await criarUseCase.ExecutarAsync("Lead2", "11922222222", null, null, null);

        var resultado = await editarUseCase.ExecutarAsync(lead2.Valor, "Lead2", "11911111111", null, null, null);

        Assert.False(resultado.Sucesso);
        Assert.Equal("Telefone já cadastrado", resultado.MensagemErro);
        Assert.Equal(EditarLeadErro.TelefoneDuplicado, resultado.Erro);
    }

    [Fact]
    public async Task NaoDeveFalhar_QuandoEditaSemMudarOTelefone()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var editarUseCase = new EditarLeadUseCase(repositorio);
        var criado = await criarUseCase.ExecutarAsync("Fulano", "11911111111", null, null, null);

        var resultado = await editarUseCase.ExecutarAsync(criado.Valor, "Fulano Editado", "11911111111", null, null, null);

        Assert.True(resultado.Sucesso);
    }

    [Fact]
    public async Task DeveReaproveitarOMesmoEndereco_QuandoLeadJaTemEnderecoEEEditadoComNovosDados()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var editarUseCase = new EditarLeadUseCase(repositorio);
        var enderecoOriginal = new EnderecoInput("Rua A", "100", null, "Centro", "São Paulo", "SP", "01000-000");
        var criado = await criarUseCase.ExecutarAsync("Fulano", "11911111111", null, enderecoOriginal, null);
        var leadCriado = await repositorio.BuscarPorIdAsync(criado.Valor);
        var enderecoIdOriginal = leadCriado!.Endereco!.Id;

        var enderecoNovo = new EnderecoInput("Rua B", "200", "Apto 3", "Jardins", "Rio de Janeiro", "RJ", "20000-000");
        var resultado = await editarUseCase.ExecutarAsync(criado.Valor, "Fulano", "11911111111", null, enderecoNovo, null);

        Assert.True(resultado.Sucesso);
        var atualizado = await repositorio.BuscarPorIdAsync(criado.Valor);
        Assert.NotNull(atualizado!.Endereco);
        Assert.Equal(enderecoIdOriginal, atualizado.Endereco!.Id);
        Assert.Equal("Rua B", atualizado.Endereco.Rua);
        Assert.Equal("Rio de Janeiro", atualizado.Endereco.Cidade);
        Assert.Same(leadCriado.Endereco, atualizado.Endereco);
    }

    [Fact]
    public async Task DeveCriarNovoEndereco_QuandoLeadNaoTinhaEnderecoAnterior()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var editarUseCase = new EditarLeadUseCase(repositorio);
        var criado = await criarUseCase.ExecutarAsync("Fulano", "11911111111", null, null, null);

        var enderecoNovo = new EnderecoInput("Rua B", "200", null, "Jardins", "Rio de Janeiro", "RJ", "20000-000");
        var resultado = await editarUseCase.ExecutarAsync(criado.Valor, "Fulano", "11911111111", null, enderecoNovo, null);

        Assert.True(resultado.Sucesso);
        var atualizado = await repositorio.BuscarPorIdAsync(criado.Valor);
        Assert.NotNull(atualizado!.Endereco);
        Assert.Equal("Rua B", atualizado.Endereco!.Rua);
    }

    [Fact]
    public async Task DeveLimparEndereco_QuandoEnderecoInformadoComoNulo()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var editarUseCase = new EditarLeadUseCase(repositorio);
        var enderecoOriginal = new EnderecoInput("Rua A", "100", null, "Centro", "São Paulo", "SP", "01000-000");
        var criado = await criarUseCase.ExecutarAsync("Fulano", "11911111111", null, enderecoOriginal, null);

        var resultado = await editarUseCase.ExecutarAsync(criado.Valor, "Fulano", "11911111111", null, null, null);

        Assert.True(resultado.Sucesso);
        var atualizado = await repositorio.BuscarPorIdAsync(criado.Valor);
        Assert.Null(atualizado!.Endereco);
    }

    [Fact]
    public async Task DeveAtualizarGrupoId_QuandoInformadoNaEdicao()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var editarUseCase = new EditarLeadUseCase(repositorio);
        var criado = await criarUseCase.ExecutarAsync("Fulano", "11911111111", null, null, null);
        var grupoId = Guid.NewGuid();

        var resultado = await editarUseCase.ExecutarAsync(criado.Valor, "Fulano", "11911111111", null, null, null, grupoId);

        Assert.True(resultado.Sucesso);
        var atualizado = await repositorio.BuscarPorIdAsync(criado.Valor);
        Assert.Equal(grupoId, atualizado!.GrupoId);
    }
}
