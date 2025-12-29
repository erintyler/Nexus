using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Nexus.Aspire.IntegrationTests;

/// <summary>
/// Integration tests for the ImageProcessor background service using .NET Aspire testing infrastructure.
/// These tests validate the image processing worker with full dependency orchestration.
/// </summary>
public class ImageProcessorServiceTests
{
    [Fact]
    public async Task ImageProcessorService_StartsSuccessfully()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        // Act
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Assert - Wait for the ImageProcessor service to be healthy
        await app.ResourceNotifications.WaitForResourceHealthyAsync("nexus-imageprocessor")
            .WaitAsync(TimeSpan.FromSeconds(60));
    }

    [Fact]
    public async Task ImageProcessorService_WithDependencies_StartsAfterPrerequisites()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act - Verify dependencies start first
        await app.ResourceNotifications.WaitForResourceHealthyAsync("postgres")
            .WaitAsync(TimeSpan.FromSeconds(90));

        await app.ResourceNotifications.WaitForResourceHealthyAsync("rabbitmq")
            .WaitAsync(TimeSpan.FromSeconds(90));

        // Assert - Then ImageProcessor starts
        await app.ResourceNotifications.WaitForResourceHealthyAsync("nexus-imageprocessor")
            .WaitAsync(TimeSpan.FromSeconds(90));
    }

    [Fact]
    public async Task ImageProcessorService_HealthCheck_IsAvailable()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Act - Wait for the ImageProcessor to be ready
        await app.ResourceNotifications.WaitForResourceHealthyAsync("nexus-imageprocessor")
            .WaitAsync(TimeSpan.FromSeconds(60));

        // Get HTTP client for the ImageProcessor service
        var httpClient = app.CreateHttpClient("nexus-imageprocessor");

        // Assert - Check health endpoint
        var response = await httpClient.GetAsync("/health");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }
}
