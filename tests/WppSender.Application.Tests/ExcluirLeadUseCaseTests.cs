using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using Xunit;

namespace WppSender.Application.Tests;

public class ExcluirLeadUseCaseTests
{
    [Fact]
    public async Task DeveMarcarComoExcluido_QuandoLeadExisteEEstaAtivo()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var excluirUseCase = new ExcluirLeadUseCase(repositorio);
        var criado = await criarUseCase.ExecutarAsync("Fulano", "11911111111", null, null, null);

        var resultado = await excluirUseCase.ExecutarAsync(criado.LeadId!.Value);

        Assert.True(resultado.Sucesso);
        var lead = await repositorio.BuscarPorIdAsync(criado.LeadId.Value);
        Assert.False(lead!.EstaAtivo);
        Assert.NotNull(lead.DeletadoEm);
    }

    [Fact]
    public async Task NaoDeveAparecerNaListagem_AposExcluir()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var excluirUseCase = new ExcluirLeadUseCase(repositorio);
        var criado = await criarUseCase.ExecutarAsync("Fulano", "11911111111", null, null, null);

        await excluirUseCase.ExecutarAsync(criado.LeadId!.Value);

        var (itens, total) = await repositorio.ListarAsync(null, 1, 10);
        Assert.Empty(itens);
        Assert.Equal(0, total);
    }

    [Fact]
    public async Task DeveRetornarFalha_QuandoLeadNaoExiste()
    {
        var repositorio = new FakeLeadRepository();
        var excluirUseCase = new ExcluirLeadUseCase(repositorio);

        var resultado = await excluirUseCase.ExecutarAsync(Guid.NewGuid());

        Assert.False(resultado.Sucesso);
        Assert.Equal("Lead não encontrado", resultado.MensagemErro);
    }

    [Fact]
    public async Task DeveRetornarFalha_QuandoLeadJaEstaExcluido()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var excluirUseCase = new ExcluirLeadUseCase(repositorio);
        var criado = await criarUseCase.ExecutarAsync("Fulano", "11911111111", null, null, null);
        await excluirUseCase.ExecutarAsync(criado.LeadId!.Value);

        var resultado = await excluirUseCase.ExecutarAsync(criado.LeadId.Value);

        Assert.False(resultado.Sucesso);
        Assert.Equal("Lead não encontrado", resultado.MensagemErro);
    }
}
