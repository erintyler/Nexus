using Alba;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Nexus.IntegrationTests.Utilities.Fixtures;
using Xunit;

namespace Nexus.Api.IntegrationTests.Fixtures;

/// <summary>
/// Alba fixture for integration testing the API.
/// Each test instance gets its own fresh PostgreSQL and RabbitMQ containers
/// for complete isolation and no interference between tests.
/// </summary>
public sealed class AlbaWebApplicationFixture : IAsyncLifetime
{
    private PostgresContainerFixture _postgresFixture = null!;
    private RabbitMqContainerFixture _rabbitMqFixture = null!;
    private IAlbaHost _host = null!;

    public async ValueTask InitializeAsync()
    {
        // Create fresh containers for this test
        _postgresFixture = new PostgresContainerFixture();
        _rabbitMqFixture = new RabbitMqContainerFixture();

        await _postgresFixture.InitializeAsync();
        await _rabbitMqFixture.InitializeAsync();

        // Set the connection strings in environment variables so Aspire can pick them up
        Environment.SetEnvironmentVariable("ConnectionStrings__postgres", _postgresFixture.ConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__rabbitmq", _rabbitMqFixture.ConnectionString);

        // Create Alba host with fresh containers
        _host = await AlbaHost.For<Program>();

        // Get document store and apply schema
        var documentStore = _host.Services.GetRequiredService<IDocumentStore>();
        await documentStore.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
    }

    /// <summary>
    /// Gets the Alba host for this test instance.
    /// Each test gets its own host with fresh database and RabbitMQ containers.
    /// </summary>
    public IAlbaHost Host => _host;

    public async ValueTask DisposeAsync()
    {
        // Clean up host first
        if (_host != null)
        {
            await _host.DisposeAsync();
        }

        // Then clean up containers
        if (_rabbitMqFixture != null)
        {
            await _rabbitMqFixture.DisposeAsync();
        }

        if (_postgresFixture != null)
        {
            await _postgresFixture.DisposeAsync();
        }

        // Clean up environment variables
        Environment.SetEnvironmentVariable("ConnectionStrings__postgres", null);
        Environment.SetEnvironmentVariable("ConnectionStrings__rabbitmq", null);
    }
}
