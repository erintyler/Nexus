using System.Net;
using Alba;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Application.Features.Auth.ExchangeToken;
using Xunit;

namespace Nexus.Api.IntegrationTests;

/// <summary>
/// Integration tests for Auth API endpoints.
/// Tests the full HTTP request/response cycle including Wolverine and Marten.
/// Each test gets its own isolated Alba host instance with fresh database and RabbitMQ containers.
/// </summary>
public class AuthEndpointsTests : IAsyncLifetime
{
    private readonly AlbaWebApplicationFixture _fixture = new();

    public async ValueTask InitializeAsync()
    {
        await _fixture.InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    [Fact]
    public async Task ExchangeToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var command = new ExchangeTokenCommand(
            AccessToken: "invalid-token"
        );

        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/auth/exchange");
            scenario.StatusCodeShouldBe(HttpStatusCode.Unauthorized);
        });
    }

    [Fact]
    public async Task ExchangeToken_WithEmptyToken_ReturnsBadRequest()
    {
        // Arrange
        var command = new ExchangeTokenCommand(
            AccessToken: ""
        );

        // Act & Assert
        await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/auth/exchange");
            scenario.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }
}
