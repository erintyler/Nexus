using Alba;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Nexus.IntegrationTests.Utilities.Fixtures;
using Xunit;

namespace Nexus.Api.IntegrationTests.Fixtures;

/// <summary>
/// Alba fixture for integration testing the API.
/// This creates a test host with a real PostgreSQL database using Testcontainers.
/// </summary>
public sealed class AlbaWebApplicationFixture : IAsyncLifetime
{
    private readonly PostgresContainerFixture _postgresFixture = new();
    private readonly RabbitMqContainerFixture _rabbitMqFixture = new();
    private IAlbaHost _host = null!;

    public IAlbaHost Host => _host;
    public string ConnectionString => _postgresFixture.ConnectionString;

    public async ValueTask InitializeAsync()
    {
        await _postgresFixture.InitializeAsync();
        await _rabbitMqFixture.InitializeAsync();

        // Set the connection strings in environment variables so Aspire can pick them up
        Environment.SetEnvironmentVariable("ConnectionStrings__postgres", _postgresFixture.ConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__rabbitmq", _rabbitMqFixture.ConnectionString);

        _host = await AlbaHost.For<Program>();

        // Get document store and apply schema
        var documentStore = _host.Services.GetRequiredService<IDocumentStore>();
        await documentStore.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _host.DisposeAsync();
        await _rabbitMqFixture.DisposeAsync();
        await _postgresFixture.DisposeAsync();

        // Clean up environment variables
        Environment.SetEnvironmentVariable("ConnectionStrings__postgres", null);
        Environment.SetEnvironmentVariable("ConnectionStrings__rabbitmq", null);
    }
}
