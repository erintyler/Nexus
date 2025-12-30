using Alba;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Application.Features.Auth.ExchangeToken;
using Xunit;

namespace Nexus.Api.IntegrationTests.Tests;

public class AuthEndpointsTests : IClassFixture<AlbaWebApplicationFixture>
{
    private readonly AlbaWebApplicationFixture _fixture;

    public AuthEndpointsTests(AlbaWebApplicationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ExchangeToken_WithEmptyAccessToken_ReturnsBadRequest()
    {
        // Arrange
        var command = new ExchangeTokenCommand(AccessToken: "");

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/auth/exchange");
            scenario.StatusCodeShouldBe(422);
        });
    }

    [Fact]
    public async Task ExchangeToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange - Use an invalid token
        var command = new ExchangeTokenCommand(AccessToken: "invalid-token-123");

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/auth/exchange");
            scenario.StatusCodeShouldBe(401);
        });
    }

    [Fact]
    public async Task ExchangeToken_WithNullAccessToken_ReturnsBadRequest()
    {
        // Arrange
        var command = new ExchangeTokenCommand(AccessToken: null!);

        // Act & Assert
        await _fixture.AlbaHost.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/auth/exchange");
            scenario.StatusCodeShouldBe(422);
        });
    }
}
