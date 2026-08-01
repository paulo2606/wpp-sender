using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using Xunit;

namespace WppSender.Application.Tests;

public class ExportarLeadsCsvUseCaseTests
{
    [Fact]
    public async Task DeveEnviarTodosOsLeadsAtivosParaOWriter()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        await criarUseCase.ExecutarAsync("Ana", "11911111111", null, null, null);
        await criarUseCase.ExecutarAsync("Bruno", "11922222222", null, null, null);
        var writer = new FakeLeadCsvWriter();
        var useCase = new ExportarLeadsCsvUseCase(repositorio, writer);

        await useCase.ExecutarAsync(Stream.Null);

        Assert.Equal(2, writer.LeadsRecebidos.Count);
    }

    [Fact]
    public async Task NaoDeveEnviarLeadsExcluidosParaOWriter()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        var excluirUseCase = new ExcluirLeadUseCase(repositorio);
        var ativo = await criarUseCase.ExecutarAsync("Ana", "11911111111", null, null, null);
        var excluido = await criarUseCase.ExecutarAsync("Bruno", "11922222222", null, null, null);
        await excluirUseCase.ExecutarAsync(excluido.LeadId!.Value);
        var writer = new FakeLeadCsvWriter();
        var useCase = new ExportarLeadsCsvUseCase(repositorio, writer);

        await useCase.ExecutarAsync(Stream.Null);

        Assert.Single(writer.LeadsRecebidos);
        Assert.Equal(ativo.LeadId, writer.LeadsRecebidos[0].Id);
    }
}
