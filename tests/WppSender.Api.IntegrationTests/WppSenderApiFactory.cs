using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using WppSender.Application.Campanhas;
using WppSender.Infrastructure.Persistence;

namespace WppSender.Api.IntegrationTests;

public class WppSenderApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WppSenderDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync() => await _container.DisposeAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _container.GetConnectionString(),
                ["Jwt:SigningKey"] = "chave-de-teste-com-pelo-menos-32-caracteres!!",
                ["Jwt:ExpirationMinutes"] = "60"
            });
        });

        // Não há serviço WhatsApp/Baileys real disponível em ambiente de teste, então o
        // HttpWhatsAppClient (que dependeria de uma chamada HTTP real) é substituído por
        // um fake determinístico para todos os testes de integração.
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IWhatsAppClient>();
            services.AddScoped<IWhatsAppClient, FakeWhatsAppClient>();
        });
    }
}
