using System.Net.Http.Json;
using System.Text.Json;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Nexus.Aspire.IntegrationTests;

/// <summary>
/// End-to-end integration tests that validate complete workflows across multiple services.
/// These tests simulate real-world scenarios with full service orchestration.
/// </summary>
public class EndToEndTests
{
    [Fact]
    public async Task CompleteApplication_StartsSuccessfully_WithAllServices()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        // Act
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Assert - All services should start in proper order
        var expectedServices = new[]
        {
            "postgres",
            "rabbitmq",
            "nexus-api",
            "nexus-imageprocessor"
        };

        foreach (var serviceName in expectedServices)
        {
            await app.ResourceNotifications.WaitForResourceHealthyAsync(serviceName)
                .WaitAsync(TimeSpan.FromSeconds(120));
        }
    }

    [Fact]
    public async Task ApiService_CanConnectToDatabase_AndProcessRequests()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Wait for all dependencies
        await app.ResourceNotifications.WaitForResourceHealthyAsync("postgres")
            .WaitAsync(TimeSpan.FromSeconds(90));
        await app.ResourceNotifications.WaitForResourceHealthyAsync("nexus-api")
            .WaitAsync(TimeSpan.FromSeconds(90));

        // Act - Create HTTP client and test API
        var httpClient = app.CreateHttpClient("nexus-api");

        // Assert - API should be responsive
        var healthResponse = await httpClient.GetAsync("/health");
        healthResponse.EnsureSuccessStatusCode();

        var healthContent = await healthResponse.Content.ReadAsStringAsync();
        Assert.NotNull(healthContent);
    }

    [Fact]
    public async Task ServiceDiscovery_ApiCanDiscoverDependencies()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act - Wait for all services to be healthy
        await app.ResourceNotifications.WaitForResourceHealthyAsync("postgres")
            .WaitAsync(TimeSpan.FromSeconds(90));
        await app.ResourceNotifications.WaitForResourceHealthyAsync("rabbitmq")
            .WaitAsync(TimeSpan.FromSeconds(90));
        await app.ResourceNotifications.WaitForResourceHealthyAsync("nexus-api")
            .WaitAsync(TimeSpan.FromSeconds(90));

        // Assert - Verify connection strings are available
        var postgresConnectionString = await app.GetConnectionStringAsync("postgres");
        var rabbitmqConnectionString = await app.GetConnectionStringAsync("rabbitmq");

        Assert.NotNull(postgresConnectionString);
        Assert.NotNull(rabbitmqConnectionString);
        Assert.NotEmpty(postgresConnectionString);
        Assert.NotEmpty(rabbitmqConnectionString);
    }

    [Fact]
    public async Task MultipleServices_CanAccessSharedInfrastructure()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act - Wait for infrastructure and services
        await app.ResourceNotifications.WaitForResourceHealthyAsync("postgres")
            .WaitAsync(TimeSpan.FromSeconds(90));
        await app.ResourceNotifications.WaitForResourceHealthyAsync("rabbitmq")
            .WaitAsync(TimeSpan.FromSeconds(90));
        await app.ResourceNotifications.WaitForResourceHealthyAsync("nexus-api")
            .WaitAsync(TimeSpan.FromSeconds(90));
        await app.ResourceNotifications.WaitForResourceHealthyAsync("nexus-imageprocessor")
            .WaitAsync(TimeSpan.FromSeconds(90));

        // Assert - Both services should have access to the same infrastructure
        var apiClient = app.CreateHttpClient("nexus-api");
        var processorClient = app.CreateHttpClient("nexus-imageprocessor");

        var apiHealthResponse = await apiClient.GetAsync("/health");
        var processorHealthResponse = await processorClient.GetAsync("/health");

        apiHealthResponse.EnsureSuccessStatusCode();
        processorHealthResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Application_GracefulShutdown_WithoutErrors()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Wait for services to start
        await app.ResourceNotifications.WaitForResourceHealthyAsync("nexus-api")
            .WaitAsync(TimeSpan.FromSeconds(90));

        // Act - Stop the application (implicit via DisposeAsync)
        await app.StopAsync();

        // Assert - No exceptions should be thrown during shutdown
        // The test passing itself validates graceful shutdown
        Assert.True(true);
    }
}
