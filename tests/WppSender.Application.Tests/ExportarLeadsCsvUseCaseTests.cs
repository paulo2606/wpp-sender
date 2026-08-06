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
        var useCase = new ExportarLeadsCsvUseCase(repositorio, new FakeGrupoRepository(), writer);

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
        await excluirUseCase.ExecutarAsync(excluido.Valor);
        var writer = new FakeLeadCsvWriter();
        var useCase = new ExportarLeadsCsvUseCase(repositorio, new FakeGrupoRepository(), writer);

        await useCase.ExecutarAsync(Stream.Null);

        Assert.Single(writer.LeadsRecebidos);
        Assert.Equal(ativo.Valor, writer.LeadsRecebidos[0].Id);
    }

    [Fact]
    public async Task DeveIterarPorTodasAsPaginas_QuandoTotalExcedeOTamanhoDaPagina()
    {
        var repositorio = new FakeLeadRepository();
        var criarUseCase = new CriarLeadUseCase(repositorio);
        for (var i = 0; i < 5; i++)
        {
            await criarUseCase.ExecutarAsync($"Lead{i}", $"1191111000{i}", null, null, null);
        }
        var writer = new FakeLeadCsvWriter();
        var useCase = new ExportarLeadsCsvUseCase(repositorio, new FakeGrupoRepository(), writer, tamanhoPagina: 2);

        await useCase.ExecutarAsync(Stream.Null);

        Assert.Equal(5, writer.LeadsRecebidos.Count);
        Assert.Equal(5, writer.LeadsRecebidos.Select(l => l.Id).Distinct().Count());
    }

    [Fact]
    public async Task DeveResolverNomeDoGrupo_QuandoLeadPertenceAUmGrupo()
    {
        var leadRepositorio = new FakeLeadRepository();
        var grupoRepositorio = new FakeGrupoRepository();
        var grupo = new WppSender.Domain.Grupo(Guid.NewGuid(), "Clientes VIP", null);
        await grupoRepositorio.AdicionarAsync(grupo);
        var criarUseCase = new CriarLeadUseCase(leadRepositorio);
        await criarUseCase.ExecutarAsync("ComGrupo", "11911111111", null, null, null, grupo.Id);
        await criarUseCase.ExecutarAsync("SemGrupo", "11922222222", null, null, null);
        var writer = new FakeLeadCsvWriter();
        var useCase = new ExportarLeadsCsvUseCase(leadRepositorio, grupoRepositorio, writer);

        await useCase.ExecutarAsync(Stream.Null);

        var comGrupo = writer.LeadsExportados.Single(e => e.Lead.Nome == "ComGrupo");
        var semGrupo = writer.LeadsExportados.Single(e => e.Lead.Nome == "SemGrupo");
        Assert.Equal("Clientes VIP", comGrupo.NomeGrupo);
        Assert.Null(semGrupo.NomeGrupo);
    }

    [Fact]
    public async Task DeveFiltrarPorGrupoId_QuandoInformado()
    {
        var leadRepositorio = new FakeLeadRepository();
        var grupoRepositorio = new FakeGrupoRepository();
        var grupo = new WppSender.Domain.Grupo(Guid.NewGuid(), "Grupo A", null);
        await grupoRepositorio.AdicionarAsync(grupo);
        var criarUseCase = new CriarLeadUseCase(leadRepositorio);
        await criarUseCase.ExecutarAsync("DoGrupo", "11911111111", null, null, null, grupo.Id);
        await criarUseCase.ExecutarAsync("SemGrupo", "11922222222", null, null, null);
        var writer = new FakeLeadCsvWriter();
        var useCase = new ExportarLeadsCsvUseCase(leadRepositorio, grupoRepositorio, writer);

        await useCase.ExecutarAsync(Stream.Null, grupo.Id);

        Assert.Single(writer.LeadsExportados);
        Assert.Equal("DoGrupo", writer.LeadsExportados[0].Lead.Nome);
    }
}
