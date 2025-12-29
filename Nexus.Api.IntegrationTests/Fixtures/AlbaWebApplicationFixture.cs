using Alba;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Nexus.IntegrationTests.Utilities.Fixtures;
using Xunit;

namespace Nexus.Api.IntegrationTests.Fixtures;

/// <summary>
/// Alba fixture for integration testing the API.
/// Creates infrastructure (PostgreSQL and RabbitMQ containers) shared across tests,
/// but each test gets its own Alba host instance for proper isolation.
/// </summary>
public sealed class AlbaWebApplicationFixture : IAsyncLifetime
{
    private readonly PostgresContainerFixture _postgresFixture = new();
    private readonly RabbitMqContainerFixture _rabbitMqFixture = new();

    public string PostgresConnectionString => _postgresFixture.ConnectionString;
    public string RabbitMqConnectionString => _rabbitMqFixture.ConnectionString;

    public async ValueTask InitializeAsync()
    {
        await _postgresFixture.InitializeAsync();
        await _rabbitMqFixture.InitializeAsync();
    }

    /// <summary>
    /// Creates a new Alba host for a single test.
    /// Each test should call this to get its own isolated host instance.
    /// </summary>
    public async Task<IAlbaHost> CreateHost()
    {
        // Set the connection strings in environment variables so Aspire can pick them up
        Environment.SetEnvironmentVariable("ConnectionStrings__postgres", PostgresConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__rabbitmq", RabbitMqConnectionString);

        var host = await AlbaHost.For<Program>();

        // Get document store and apply schema
        var documentStore = host.Services.GetRequiredService<IDocumentStore>();
        await documentStore.Storage.ApplyAllConfiguredChangesToDatabaseAsync();

        // Clean up database for this test
        await documentStore.Advanced.Clean.DeleteAllDocumentsAsync();
        await documentStore.Advanced.Clean.DeleteAllEventDataAsync();

        return host;
    }

    public async ValueTask DisposeAsync()
    {
        await _rabbitMqFixture.DisposeAsync();
        await _postgresFixture.DisposeAsync();

        // Clean up environment variables
        Environment.SetEnvironmentVariable("ConnectionStrings__postgres", null);
        Environment.SetEnvironmentVariable("ConnectionStrings__rabbitmq", null);
    }
}
