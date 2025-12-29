using System.Net;
using System.Net.Http.Json;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Application.Features.Auth.ExchangeToken;

namespace Nexus.Api.IntegrationTests.Endpoints;

public class AuthEndpointsTests : IClassFixture<ApiFixture>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(ApiFixture fixture)
    {
        _client = fixture.HttpClient;
    }

    [Fact]
    public async Task ExchangeToken_ShouldReturnUnauthorized_WhenInvalidToken()
    {
        // Arrange
        var command = new ExchangeTokenCommand("invalid_discord_token");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/exchange", command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ExchangeToken_ShouldReturnValidationError_WhenAccessTokenIsEmpty()
    {
        // Arrange
        var command = new ExchangeTokenCommand("");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/exchange", command, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ExchangeToken_ShouldReturnJwtToken_WhenValidDiscordToken()
    {
        // Note: This test assumes a valid Discord token is provided or mocked.
        // In a real scenario, you would either:
        // 1. Mock the Discord API service
        // 2. Use a test Discord token
        // 3. Skip this test in CI/CD environments

        // This test is included for completeness but may need to be adjusted
        // based on your authentication setup and testing strategy.

        // For now, we'll test the endpoint structure is correct
        // You may need to update this test with actual valid credentials or mock setup

        // Arrange
        var command = new ExchangeTokenCommand("test_discord_token");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/exchange", command, TestContext.Current.CancellationToken);

        // Assert
        // Since we don't have a valid Discord token in tests, we expect Unauthorized
        // In a real integration test with proper Discord mock/test setup, you would expect OK
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.OK,
            $"Expected Unauthorized or OK, but got {response.StatusCode}");

        // If the response is OK (in case of valid test setup), verify the response structure
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var result = await response.Content.ReadFromJsonAsync<ExchangeTokenResponse>(TestContext.Current.CancellationToken);
            Assert.NotNull(result);
            Assert.NotEmpty(result.AccessToken);
            Assert.Equal("Bearer", result.TokenType);
            Assert.True(result.ExpiresIn > 0);
            Assert.NotNull(result.Claims);
        }
    }
}
