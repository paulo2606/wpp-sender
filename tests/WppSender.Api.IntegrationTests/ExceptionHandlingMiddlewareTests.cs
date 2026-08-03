using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using WppSender.Api.Middleware;
using Xunit;

namespace WppSender.Api.IntegrationTests;

public class ExceptionHandlingMiddlewareTests
{
    private class LoggerDeTeste : ILogger<ExceptionHandlingMiddleware>
    {
        public bool ErroRegistrado { get; private set; }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Error && exception is not null)
            {
                ErroRegistrado = true;
            }
        }

        private class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }

    [Fact]
    public async Task InvokeAsync_DeveRegistrarErroNoLogger_QuandoProximoMiddlewareLancaExcecao()
    {
        var logger = new LoggerDeTeste();
        var middleware = new ExceptionHandlingMiddleware(_ => throw new InvalidOperationException("Falha simulada"), logger);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.True(logger.ErroRegistrado);
        Assert.Equal(500, context.Response.StatusCode);
    }
}
