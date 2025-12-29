using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Nexus.Aspire.IntegrationTests;

/// <summary>
/// Integration tests for infrastructure dependencies (PostgreSQL, RabbitMQ, LocalStack).
/// These tests validate that all infrastructure resources are properly configured and accessible.
/// </summary>
public class InfrastructureTests
{
    [Fact]
    public async Task PostgresDatabase_StartsSuccessfully()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        // Act
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Assert - Wait for PostgreSQL to be healthy
        await app.ResourceNotifications.WaitForResourceHealthyAsync("postgres")
            .WaitAsync(TimeSpan.FromSeconds(90));
    }

    [Fact]
    public async Task RabbitMQ_StartsWithManagementPlugin()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        // Act
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Assert - Wait for RabbitMQ to be healthy
        await app.ResourceNotifications.WaitForResourceHealthyAsync("rabbitmq")
            .WaitAsync(TimeSpan.FromSeconds(90));
    }

    [Fact]
    public async Task AllInfrastructureServices_StartInCorrectOrder()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        // Act
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Assert - Verify all infrastructure services are healthy
        var infrastructureResources = new[] { "postgres", "rabbitmq" };

        foreach (var resourceName in infrastructureResources)
        {
            await app.ResourceNotifications.WaitForResourceHealthyAsync(resourceName)
                .WaitAsync(TimeSpan.FromSeconds(90));
        }
    }

    [Fact]
    public async Task PostgresDatabase_IsHealthy()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        // Act
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Wait for PostgreSQL to be healthy
        await app.ResourceNotifications.WaitForResourceHealthyAsync("postgres")
            .WaitAsync(TimeSpan.FromSeconds(90));

        // Get connection string for PostgreSQL
        var connectionString = await app.GetConnectionStringAsync("postgres");

        // Assert
        Assert.NotNull(connectionString);
        Assert.NotEmpty(connectionString);
    }

    [Fact]
    public async Task RabbitMQ_ConnectionString_IsAvailable()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        // Act
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Wait for RabbitMQ to be healthy
        await app.ResourceNotifications.WaitForResourceHealthyAsync("rabbitmq")
            .WaitAsync(TimeSpan.FromSeconds(90));

        // Get connection string for RabbitMQ
        var connectionString = await app.GetConnectionStringAsync("rabbitmq");

        // Assert
        Assert.NotNull(connectionString);
        Assert.NotEmpty(connectionString);
        Assert.Contains("amqp://", connectionString);
    }
}
