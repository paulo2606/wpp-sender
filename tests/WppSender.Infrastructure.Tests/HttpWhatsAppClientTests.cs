using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using WppSender.Domain;
using WppSender.Infrastructure.WhatsApp;

namespace WppSender.Infrastructure.Tests;

public class HttpWhatsAppClientTests
{
    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public HttpRequestMessage? UltimaRequisicao { get; private set; }

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            UltimaRequisicao = request;
            return Task.FromResult(_handler(request));
        }
    }

    private class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("Falha de conexao simulada");
        }
    }

    private static HttpWhatsAppClient CriarCliente(HttpMessageHandler handler, string apiKey = "chave-teste")
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:3000/") };
        httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        return new HttpWhatsAppClient(httpClient, NullLogger<HttpWhatsAppClient>.Instance);
    }

    [Fact]
    public async Task EnviarMensagemAsync_DeveIncluirHeaderDeApiKey()
    {
        var fakeHandler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var cliente = CriarCliente(fakeHandler, "minha-chave-secreta");

        await cliente.EnviarMensagemAsync("5511912345678", "Ola");

        Assert.True(fakeHandler.UltimaRequisicao!.Headers.Contains("X-Api-Key"));
        Assert.Equal("minha-chave-secreta", fakeHandler.UltimaRequisicao.Headers.GetValues("X-Api-Key").Single());
    }

    [Fact]
    public async Task EnviarMensagemAsync_DeveRetornarSucesso_QuandoRespostaEh2xx()
    {
        var fakeHandler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var cliente = CriarCliente(fakeHandler);

        var resultado = await cliente.EnviarMensagemAsync("5511912345678", "Ola");

        Assert.True(resultado.Sucesso);
    }

    [Fact]
    public async Task EnviarMensagemAsync_DeveRetornarFalhaComMensagem_QuandoRespostaNaoEh2xx()
    {
        var fakeHandler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("erro interno", Encoding.UTF8),
        });
        var cliente = CriarCliente(fakeHandler);

        var resultado = await cliente.EnviarMensagemAsync("5511912345678", "Ola");

        Assert.False(resultado.Sucesso);
        Assert.Equal("erro interno", resultado.MensagemErro);
    }

    [Fact]
    public async Task IniciarSessaoAsync_DeveIncluirHeaderDeApiKey()
    {
        var fakeHandler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { qrCodeBase64 = "abc" }), Encoding.UTF8, "application/json"),
        });
        var cliente = CriarCliente(fakeHandler, "minha-chave-secreta");

        await cliente.IniciarSessaoAsync();

        Assert.Equal("minha-chave-secreta", fakeHandler.UltimaRequisicao!.Headers.GetValues("X-Api-Key").Single());
    }

    [Fact]
    public async Task IniciarSessaoAsync_DeveRetornarQrCode_QuandoRespostaEhSucesso()
    {
        var fakeHandler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { qrCodeBase64 = "abc123" }), Encoding.UTF8, "application/json"),
        });
        var cliente = CriarCliente(fakeHandler);

        var qrCode = await cliente.IniciarSessaoAsync();

        Assert.Equal("abc123", qrCode);
    }

    [Fact]
    public async Task IniciarSessaoAsync_DeveRetornarStringVazia_QuandoServidorRespondeErro()
    {
        var fakeHandler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var cliente = CriarCliente(fakeHandler);

        var qrCode = await cliente.IniciarSessaoAsync();

        Assert.Equal(string.Empty, qrCode);
    }

    [Fact]
    public async Task IniciarSessaoAsync_DeveRetornarStringVazia_QuandoConexaoFalha()
    {
        var cliente = CriarCliente(new ThrowingHttpMessageHandler());

        var qrCode = await cliente.IniciarSessaoAsync();

        Assert.Equal(string.Empty, qrCode);
    }

    [Fact]
    public async Task ObterStatusSessaoAsync_DeveIncluirHeaderDeApiKey()
    {
        var fakeHandler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { status = "conectado" }), Encoding.UTF8, "application/json"),
        });
        var cliente = CriarCliente(fakeHandler, "minha-chave-secreta");

        await cliente.ObterStatusSessaoAsync();

        Assert.Equal("minha-chave-secreta", fakeHandler.UltimaRequisicao!.Headers.GetValues("X-Api-Key").Single());
    }

    [Theory]
    [InlineData("conectado", StatusSessaoWhatsApp.Conectado)]
    [InlineData("aguardando_qr", StatusSessaoWhatsApp.AguardandoQr)]
    [InlineData("desconectado", StatusSessaoWhatsApp.Desconectado)]
    [InlineData("qualquer_outra_coisa", StatusSessaoWhatsApp.Desconectado)]
    public async Task ObterStatusSessaoAsync_DeveMapearStatusCorretamente(string statusRetornado, StatusSessaoWhatsApp esperado)
    {
        var fakeHandler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { status = statusRetornado }), Encoding.UTF8, "application/json"),
        });
        var cliente = CriarCliente(fakeHandler);

        var status = await cliente.ObterStatusSessaoAsync();

        Assert.Equal(esperado, status);
    }

    [Fact]
    public async Task ObterStatusSessaoAsync_DeveRetornarDesconectado_QuandoServidorRespondeErro()
    {
        var fakeHandler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var cliente = CriarCliente(fakeHandler);

        var status = await cliente.ObterStatusSessaoAsync();

        Assert.Equal(StatusSessaoWhatsApp.Desconectado, status);
    }

    [Fact]
    public async Task ObterStatusSessaoAsync_DeveRetornarDesconectado_QuandoConexaoFalha()
    {
        var cliente = CriarCliente(new ThrowingHttpMessageHandler());

        var status = await cliente.ObterStatusSessaoAsync();

        Assert.Equal(StatusSessaoWhatsApp.Desconectado, status);
    }
}
