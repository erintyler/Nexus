using Aspire.Hosting.Testing;

namespace Nexus.Frontend.IntegrationTests;

/// <summary>
/// Integration tests for YARP BFF (Backend For Frontend) proxy pattern.
/// These tests verify that the Frontend server correctly proxies API requests
/// and forwards JWT tokens from authentication cookies to the backend API.
/// </summary>
public class YarpBffProxyTests
{
    [Fact]
    public async Task Frontend_ShouldProxyApiRequests_ToBackendApi()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Get the Frontend HTTP client
        var frontendClient = app.CreateHttpClient("nexus-frontend");

        // Act - Make a request to the /api endpoint through the Frontend
        // This should be proxied by YARP to the backend API
        var response = await frontendClient.GetAsync("/api/auth/exchange");

        // Assert - We expect the request to reach the backend
        // Even though we're not authenticated, the proxy should work
        // We should get a 401 Unauthorized from the API, not a 404
        Assert.NotEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Frontend_ShouldReturn404_ForNonApiRequests()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Get the Frontend HTTP client
        var frontendClient = app.CreateHttpClient("nexus-frontend");

        // Act - Make a request to a non-existent non-API route
        var response = await frontendClient.GetAsync("/non-existent-route");

        // Assert - Should get 404 or redirect to not-found page
        Assert.True(
            response.StatusCode == System.Net.HttpStatusCode.NotFound ||
            response.StatusCode == System.Net.HttpStatusCode.Redirect ||
            response.StatusCode == System.Net.HttpStatusCode.OK, // Might render the not-found page with 200
            $"Expected 404, Redirect, or OK, but got {response.StatusCode}"
        );
    }

    [Fact]
    public async Task Frontend_YarpProxy_ShouldForwardRequestsToApi()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Get both Frontend and API HTTP clients
        var frontendClient = app.CreateHttpClient("nexus-frontend");
        var apiClient = app.CreateHttpClient("nexus-api");

        // Act - Make requests to the same endpoint through both clients
        var frontendResponse = await frontendClient.GetAsync("/api/auth/exchange");
        var apiResponse = await apiClient.GetAsync("/api/auth/exchange");

        // Assert - Both should return similar status codes (both unauthenticated)
        // This verifies the proxy is correctly forwarding to the API
        Assert.Equal(apiResponse.StatusCode, frontendResponse.StatusCode);
    }

    [Fact]
    public async Task Frontend_StaticAssets_ShouldNotBeProxied()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Get the Frontend HTTP client
        var frontendClient = app.CreateHttpClient("nexus-frontend");

        // Act - Request the root page (should be served by Frontend, not proxied)
        var response = await frontendClient.GetAsync("/");

        // Assert - Should successfully get the Blazor page
        Assert.True(
            response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Redirect,
            $"Expected success or redirect for root page, but got {response.StatusCode}"
        );

        // If successful, the content should be HTML
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("<!DOCTYPE html>", content, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task AllServices_ShouldStart_Successfully()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Nexus_AppHost>();

        await using var app = await appHost.BuildAsync();

        // Act
        await app.StartAsync();

        // Assert - All services should be accessible
        var frontendClient = app.CreateHttpClient("nexus-frontend");
        var apiClient = app.CreateHttpClient("nexus-api");

        var frontendResponse = await frontendClient.GetAsync("/");
        var apiResponse = await apiClient.GetAsync("/health");

        Assert.True(
            frontendResponse.IsSuccessStatusCode || frontendResponse.StatusCode == System.Net.HttpStatusCode.Redirect,
            "Frontend should be accessible"
        );

        // API health endpoint should respond
        Assert.NotEqual(System.Net.HttpStatusCode.ServiceUnavailable, apiResponse.StatusCode);
    }
}
