using WppSender.Domain;
using Xunit;

namespace WppSender.Domain.Tests;

public class EnderecoTests
{
    [Fact]
    public void AtualizarDados_DeveMutarInstanciaExistente_MantendoOMesmoId()
    {
        var id = Guid.NewGuid();
        var endereco = new Endereco(id, "Rua A", "100", null, "Centro", "São Paulo", "SP", "01000-000");

        endereco.AtualizarDados("Rua B", "200", "Apto 3", "Jardins", "Rio de Janeiro", "RJ", "20000-000");

        Assert.Equal(id, endereco.Id);
        Assert.Equal("Rua B", endereco.Rua);
        Assert.Equal("200", endereco.Numero);
        Assert.Equal("Apto 3", endereco.Complemento);
        Assert.Equal("Jardins", endereco.Bairro);
        Assert.Equal("Rio de Janeiro", endereco.Cidade);
        Assert.Equal("RJ", endereco.Estado);
        Assert.Equal("20000-000", endereco.Cep);
    }
}
