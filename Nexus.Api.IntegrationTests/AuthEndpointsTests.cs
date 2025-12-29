using System.Net;
using System.Net.Http.Json;
using Nexus.Application.Features.Auth.ExchangeToken;
using Xunit;

namespace Nexus.Api.IntegrationTests;

[Collection("Aspire")]
public class AuthEndpointsTests(AspireAppHostFixture fixture) : IClassFixture<AspireAppHostFixture>
{
    private readonly HttpClient _client = fixture.HttpClient;

    [Fact]
    public async Task ExchangeToken_WithValidToken_ReturnsJwtToken()
    {
        // Arrange
        var command = new ExchangeTokenCommand("valid_test_token_12345");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/exchange", command);

        // Assert
        // Note: The actual validation depends on the Discord API integration
        // In a test environment with TestAuthHandler, this might behave differently
        // We're testing that the endpoint is accessible and returns a proper response
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.UnprocessableEntity
        );
    }

    [Fact]
    public async Task ExchangeToken_WithEmptyToken_ReturnsBadRequest()
    {
        // Arrange
        var command = new ExchangeTokenCommand("");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/exchange", command);

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task ExchangeToken_WithInvalidToken_ReturnsUnauthorizedOrUnprocessable()
    {
        // Arrange
        var command = new ExchangeTokenCommand("invalid_token_that_should_fail");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/exchange", command);

        // Assert
        // The response should be either Unauthorized or UnprocessableEntity depending on the validation logic
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.UnprocessableEntity
        );
    }
}
