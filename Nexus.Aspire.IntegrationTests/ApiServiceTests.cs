using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Nexus.Aspire.IntegrationTests;

/// <summary>
/// Integration tests for the Nexus API service using .NET Aspire testing infrastructure.
/// These tests validate the API endpoints with full dependency orchestration.
/// </summary>
public class ApiServiceTests
{
    [Fact]
    public async Task ApiService_StartsSuccessfully()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        // Act
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Assert - Wait for the API service to be healthy
        await app.ResourceNotifications.WaitForResourceHealthyAsync("nexus-api")
            .WaitAsync(TimeSpan.FromSeconds(60));
    }

    [Fact]
    public async Task ApiService_HealthEndpoint_ReturnsHealthy()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act - Wait for the API to be ready
        await app.ResourceNotifications.WaitForResourceHealthyAsync("nexus-api")
            .WaitAsync(TimeSpan.FromSeconds(60));

        // Get HTTP client for the API service
        var httpClient = app.CreateHttpClient("nexus-api");

        // Assert - Check health endpoint
        var response = await httpClient.GetAsync("/health");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }

    [Fact]
    public async Task ApiService_WithDependencies_AllServicesStart()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act & Assert - Verify all critical dependencies start
        await app.ResourceNotifications.WaitForResourceHealthyAsync("postgres")
            .WaitAsync(TimeSpan.FromSeconds(90));

        await app.ResourceNotifications.WaitForResourceHealthyAsync("rabbitmq")
            .WaitAsync(TimeSpan.FromSeconds(90));

        await app.ResourceNotifications.WaitForResourceHealthyAsync("nexus-api")
            .WaitAsync(TimeSpan.FromSeconds(90));
    }
}
