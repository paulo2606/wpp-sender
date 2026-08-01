using WppSender.Application.Leads;
using WppSender.Application.Tests.Fakes;
using Xunit;

namespace WppSender.Application.Tests;

public class ImportarLeadsCsvUseCaseTests
{
    private static LeadCsvLinha Linha(int numero, string nome, string telefone) =>
        new(numero, nome, telefone, null, null, null, null, null, null, null, null, null);

    [Fact]
    public async Task DeveImportarTodasAsLinhas_QuandoNenhumTelefoneDuplicado()
    {
        var repositorio = new FakeLeadRepository();
        var parser = new FakeLeadCsvParser(new[] { Linha(2, "Ana", "11911111111"), Linha(3, "Bruno", "11922222222") });
        var useCase = new ImportarLeadsCsvUseCase(repositorio, parser);

        var resultado = await useCase.ExecutarAsync(Stream.Null);

        Assert.Equal(2, resultado.Importados);
        Assert.Empty(resultado.Puladas);
    }

    [Fact]
    public async Task DevePular_QuandoTelefoneJaExisteNoBancoEntreAtivos()
    {
        var repositorio = new FakeLeadRepository();
        await repositorio.AdicionarAsync(new WppSender.Domain.Lead(Guid.NewGuid(), "Existente", "11911111111", null, null, null));
        var parser = new FakeLeadCsvParser(new[] { Linha(2, "Ana", "11911111111") });
        var useCase = new ImportarLeadsCsvUseCase(repositorio, parser);

        var resultado = await useCase.ExecutarAsync(Stream.Null);

        Assert.Equal(0, resultado.Importados);
        Assert.Single(resultado.Puladas);
        Assert.Equal(2, resultado.Puladas[0].NumeroLinha);
        Assert.Equal("Telefone já cadastrado", resultado.Puladas[0].Motivo);
    }

    [Fact]
    public async Task DevePular_QuandoTelefoneDuplicadoDentroDoProprioArquivo()
    {
        var repositorio = new FakeLeadRepository();
        var parser = new FakeLeadCsvParser(new[]
        {
            Linha(2, "Ana", "11911111111"),
            Linha(3, "Ana Duplicada", "(11) 91111-1111"),
        });
        var useCase = new ImportarLeadsCsvUseCase(repositorio, parser);

        var resultado = await useCase.ExecutarAsync(Stream.Null);

        Assert.Equal(1, resultado.Importados);
        Assert.Single(resultado.Puladas);
        Assert.Equal(3, resultado.Puladas[0].NumeroLinha);
    }

    [Fact]
    public async Task DeveImportar_QuandoTelefoneEraDeUmLeadJaExcluido()
    {
        var repositorio = new FakeLeadRepository();
        var leadAntigo = new WppSender.Domain.Lead(Guid.NewGuid(), "Antigo", "11911111111", null, null, null);
        await repositorio.AdicionarAsync(leadAntigo);
        leadAntigo.Excluir();
        await repositorio.AtualizarAsync(leadAntigo);
        var parser = new FakeLeadCsvParser(new[] { Linha(2, "Novo", "11911111111") });
        var useCase = new ImportarLeadsCsvUseCase(repositorio, parser);

        var resultado = await useCase.ExecutarAsync(Stream.Null);

        Assert.Equal(1, resultado.Importados);
        Assert.Empty(resultado.Puladas);
    }

    [Fact]
    public async Task DeveChamarRepositorioApenasUmaVez_QuandoTelefoneDuplicadoDentroDoProprioArquivo()
    {
        var repositorio = new FakeLeadRepository();
        var parser = new FakeLeadCsvParser(new[]
        {
            Linha(2, "Ana", "11911111111"),
            Linha(3, "Ana Duplicada", "(11) 91111-1111"),
        });
        var useCase = new ImportarLeadsCsvUseCase(repositorio, parser);

        var resultado = await useCase.ExecutarAsync(Stream.Null);

        Assert.Equal(1, resultado.Importados);
        Assert.Single(resultado.Puladas);
        Assert.Equal(1, repositorio.ChamadasBuscarPorTelefone);
    }
}
