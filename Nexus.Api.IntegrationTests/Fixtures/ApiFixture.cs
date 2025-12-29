using Alba;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace Nexus.Api.IntegrationTests.Fixtures;

public sealed class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;
    private IAlbaHost? _host;

    public ApiFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:18.1-alpine")
            .WithDatabase("nexus_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCleanUp(true)
            .Build();

        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:4.0-alpine")
            .WithUsername("guest")
            .WithPassword("guest")
            .WithCleanUp(true)
            .Build();
    }

    public IAlbaHost Host => _host ?? throw new InvalidOperationException("Host is not initialized");

    public async ValueTask InitializeAsync()
    {
        // Start containers
        await _postgresContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();

        // Set environment variables for Aspire connection strings
        Environment.SetEnvironmentVariable("ConnectionStrings__postgres", _postgresContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__rabbitmq", _rabbitMqContainer.GetConnectionString());

        // Build Alba host with test configuration
        _host = await AlbaHost.For<Program>(builder =>
        {
            // Use Development environment so resources are created automatically
            builder.UseSetting("ASPNETCORE_ENVIRONMENT", "Development");
        });

        // Initialize database schema
        using var scope = _host.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        await store.Advanced.Clean.DeleteAllDocumentsAsync();
        await store.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_host != null)
        {
            await _host.DisposeAsync();
        }

        await _postgresContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = _host!.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        await store.Advanced.Clean.DeleteAllDocumentsAsync();
    }
}
