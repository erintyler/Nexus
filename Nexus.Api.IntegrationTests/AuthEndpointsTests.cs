using System.Net;
using Alba;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Application.Features.Auth.ExchangeToken;
using Xunit;

namespace Nexus.Api.IntegrationTests;

/// <summary>
/// Integration tests for Auth API endpoints.
/// Tests the full HTTP request/response cycle including Wolverine and Marten.
/// Each test gets its own isolated Alba host instance.
/// </summary>
public class AuthEndpointsTests : IClassFixture<AlbaWebApplicationFixture>
{
    private readonly AlbaWebApplicationFixture _fixture;

    public AuthEndpointsTests(AlbaWebApplicationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ExchangeToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();

        var command = new ExchangeTokenCommand(
            AccessToken: "invalid-token"
        );

        // Act & Assert
        await host.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/auth/exchange");
            scenario.StatusCodeShouldBe(HttpStatusCode.Unauthorized);
        });
    }

    [Fact]
    public async Task ExchangeToken_WithEmptyToken_ReturnsBadRequest()
    {
        // Arrange
        await using var host = await _fixture.CreateHost();

        var command = new ExchangeTokenCommand(
            AccessToken: ""
        );

        // Act & Assert
        await host.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/auth/exchange");
            scenario.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }
}
