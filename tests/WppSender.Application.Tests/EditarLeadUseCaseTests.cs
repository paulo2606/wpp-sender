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

        var resultado = await editarUseCase.ExecutarAsync(criado.LeadId!.Value, "Fulano Editado", "11922222222", "insta_novo", null, "origem_nova");

        Assert.True(resultado.Sucesso);
        var atualizado = await repositorio.BuscarPorIdAsync(criado.LeadId.Value);
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
    }

    [Fact]
    public async Task DeveRetornarFalha_QuandoNovoTelefoneJaPertenceAOutroLeadAtivo()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var editarUseCase = new EditarLeadUseCase(repositorio);
        var lead1 = await criarUseCase.ExecutarAsync("Lead1", "11911111111", null, null, null);
        var lead2 = await criarUseCase.ExecutarAsync("Lead2", "11922222222", null, null, null);

        var resultado = await editarUseCase.ExecutarAsync(lead2.LeadId!.Value, "Lead2", "11911111111", null, null, null);

        Assert.False(resultado.Sucesso);
        Assert.Equal("Telefone já cadastrado", resultado.MensagemErro);
    }

    [Fact]
    public async Task NaoDeveFalhar_QuandoEditaSemMudarOTelefone()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var editarUseCase = new EditarLeadUseCase(repositorio);
        var criado = await criarUseCase.ExecutarAsync("Fulano", "11911111111", null, null, null);

        var resultado = await editarUseCase.ExecutarAsync(criado.LeadId!.Value, "Fulano Editado", "11911111111", null, null, null);

        Assert.True(resultado.Sucesso);
    }
}
