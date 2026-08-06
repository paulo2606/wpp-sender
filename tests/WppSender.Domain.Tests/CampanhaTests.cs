using WppSender.Domain;
using Xunit;

namespace WppSender.Domain.Tests;

public class CampanhaTests
{
    [Fact]
    public void DeveCriarComStatusRascunho_QuandoSemAgendamento()
    {
        var campanha = new Campanha(Guid.NewGuid(), "Promoção", "Olá {{nome}}", Guid.NewGuid(), agendadoPara: null);

        Assert.Equal(StatusCampanha.Rascunho, campanha.Status);
    }

    [Fact]
    public void DeveCriarComStatusAgendada_QuandoComAgendamento()
    {
        var campanha = new Campanha(Guid.NewGuid(), "Promoção", "Olá {{nome}}", Guid.NewGuid(), DateTime.UtcNow.AddDays(1));

        Assert.Equal(StatusCampanha.Agendada, campanha.Status);
    }

    [Theory]
    [InlineData("", "mensagem")]
    [InlineData("nome", "")]
    public void DeveLancarExcecao_QuandoCampoObrigatorioVazio(string nome, string mensagem)
    {
        Assert.Throws<ArgumentException>(() => new Campanha(Guid.NewGuid(), nome, mensagem, Guid.NewGuid(), null));
    }

    [Fact]
    public void DeveLancarExcecao_QuandoIntervaloMaximoMenorQueMinimo()
    {
        Assert.Throws<ArgumentException>(() => new Campanha(Guid.NewGuid(), "Nome", "Msg", Guid.NewGuid(), null, intervaloMinSegundos: 90, intervaloMaxSegundos: 30));
    }

    [Theory]
    [InlineData(StatusCampanha.Rascunho, true)]
    [InlineData(StatusCampanha.Agendada, true)]
    [InlineData(StatusCampanha.EmAndamento, false)]
    [InlineData(StatusCampanha.Pausada, false)]
    [InlineData(StatusCampanha.Concluida, false)]
    public void PodeEditar_DeveRefletirApenasRascunhoOuAgendada(StatusCampanha status, bool esperado)
    {
        var campanha = CriarCampanhaComStatus(status);

        Assert.Equal(esperado, campanha.PodeEditar());
    }

    [Fact]
    public void Iniciar_DeveMudarParaEmAndamento_QuandoRascunho()
    {
        var campanha = CriarCampanhaComStatus(StatusCampanha.Rascunho);

        campanha.Iniciar();

        Assert.Equal(StatusCampanha.EmAndamento, campanha.Status);
    }

    [Fact]
    public void Iniciar_DeveLancarExcecao_QuandoJaEmAndamento()
    {
        var campanha = CriarCampanhaComStatus(StatusCampanha.EmAndamento);

        Assert.Throws<InvalidOperationException>(() => campanha.Iniciar());
    }

    [Fact]
    public void Pausar_DeveMudarParaPausada_QuandoEmAndamento()
    {
        var campanha = CriarCampanhaComStatus(StatusCampanha.EmAndamento);

        campanha.Pausar();

        Assert.Equal(StatusCampanha.Pausada, campanha.Status);
    }

    [Fact]
    public void Pausar_DeveLancarExcecao_QuandoRascunho()
    {
        var campanha = CriarCampanhaComStatus(StatusCampanha.Rascunho);

        Assert.Throws<InvalidOperationException>(() => campanha.Pausar());
    }

    [Fact]
    public void Retomar_DeveMudarParaEmAndamento_QuandoPausada()
    {
        var campanha = CriarCampanhaComStatus(StatusCampanha.Pausada);

        campanha.Retomar();

        Assert.Equal(StatusCampanha.EmAndamento, campanha.Status);
    }

    [Fact]
    public void Retomar_DeveLancarExcecao_QuandoNaoPausada()
    {
        var campanha = CriarCampanhaComStatus(StatusCampanha.Rascunho);

        Assert.Throws<InvalidOperationException>(() => campanha.Retomar());
    }

    [Fact]
    public void Concluir_DeveMudarParaConcluida_QuandoEmAndamentoSemFalhas()
    {
        var campanha = CriarCampanhaComStatus(StatusCampanha.EmAndamento);

        campanha.Concluir(comFalhas: false);

        Assert.Equal(StatusCampanha.Concluida, campanha.Status);
    }

    [Fact]
    public void Concluir_DeveMudarParaConcluidaComFalhas_QuandoHouveFalha()
    {
        var campanha = CriarCampanhaComStatus(StatusCampanha.EmAndamento);

        campanha.Concluir(comFalhas: true);

        Assert.Equal(StatusCampanha.ConcluidaComFalhas, campanha.Status);
    }

    [Fact]
    public void Cancelar_DeveMudarParaCancelada_QuandoPausada()
    {
        var campanha = CriarCampanhaComStatus(StatusCampanha.Pausada);

        campanha.Cancelar();

        Assert.Equal(StatusCampanha.Cancelada, campanha.Status);
    }

    [Theory]
    [InlineData(StatusCampanha.Rascunho)]
    [InlineData(StatusCampanha.Agendada)]
    [InlineData(StatusCampanha.EmAndamento)]
    [InlineData(StatusCampanha.Concluida)]
    [InlineData(StatusCampanha.ConcluidaComFalhas)]
    [InlineData(StatusCampanha.Cancelada)]
    public void Cancelar_DeveLancarExcecao_QuandoNaoPausada(StatusCampanha status)
    {
        var campanha = CriarCampanhaComStatus(status);

        Assert.Throws<InvalidOperationException>(() => campanha.Cancelar());
    }

    [Fact]
    public void ReabrirParaReenvio_DeveVoltarParaEmAndamento_QuandoConcluida()
    {
        var campanha = CriarCampanhaComStatus(StatusCampanha.Concluida);

        campanha.ReabrirParaReenvio();

        Assert.Equal(StatusCampanha.EmAndamento, campanha.Status);
    }

    [Fact]
    public void ReabrirParaReenvio_DeveVoltarParaEmAndamento_QuandoConcluidaComFalhas()
    {
        var campanha = CriarCampanhaComStatus(StatusCampanha.ConcluidaComFalhas);

        campanha.ReabrirParaReenvio();

        Assert.Equal(StatusCampanha.EmAndamento, campanha.Status);
    }

    [Fact]
    public void ReabrirParaReenvio_NaoDeveAlterarStatus_QuandoNaoConcluida()
    {
        var campanha = CriarCampanhaComStatus(StatusCampanha.Pausada);

        campanha.ReabrirParaReenvio();

        Assert.Equal(StatusCampanha.Pausada, campanha.Status);
    }

    [Fact]
    public void AtualizarDados_DeveAtualizarCamposEStatus()
    {
        var campanha = CriarCampanhaComStatus(StatusCampanha.Rascunho);

        campanha.AtualizarDados("Novo nome", "Nova msg {{nome}}", DateTime.UtcNow.AddDays(2), 40, 100);

        Assert.Equal("Novo nome", campanha.Nome);
        Assert.Equal(StatusCampanha.Agendada, campanha.Status);
        Assert.Equal(40, campanha.IntervaloMinSegundos);
    }

    private static Campanha CriarCampanhaComStatus(StatusCampanha status)
    {
        var campanha = new Campanha(Guid.NewGuid(), "Nome", "Msg", Guid.NewGuid(), null);
        switch (status)
        {
            case StatusCampanha.Agendada:
                campanha.AtualizarDados(campanha.Nome, campanha.Mensagem, DateTime.UtcNow.AddDays(1), campanha.IntervaloMinSegundos, campanha.IntervaloMaxSegundos);
                break;
            case StatusCampanha.EmAndamento:
                campanha.Iniciar();
                break;
            case StatusCampanha.Pausada:
                campanha.Iniciar();
                campanha.Pausar();
                break;
            case StatusCampanha.Concluida:
                campanha.Iniciar();
                campanha.Concluir(comFalhas: false);
                break;
            case StatusCampanha.ConcluidaComFalhas:
                campanha.Iniciar();
                campanha.Concluir(comFalhas: true);
                break;
        }

        return campanha;
    }
}
