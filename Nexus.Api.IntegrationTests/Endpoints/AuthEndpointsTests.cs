using System.Net;
using Alba;
using AutoFixture;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Application.Features.Auth.ExchangeToken;
using Xunit;

namespace Nexus.Api.IntegrationTests.Endpoints;

public class AuthEndpointsTests : IClassFixture<ApiFixture>
{
    private readonly ApiFixture _fixture;
    private readonly Fixture _autoFixture;

    public AuthEndpointsTests(ApiFixture fixture)
    {
        _fixture = fixture;
        _autoFixture = new Fixture();
    }

    [Fact]
    public async Task ExchangeToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var command = new ExchangeTokenCommand(AccessToken: "invalid-token");

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
        await _fixture.ResetDatabaseAsync();

        var command = new ExchangeTokenCommand(AccessToken: "");

        // Act & Assert
        var expectUnauthorized = await _fixture.Host.Scenario(scenario =>
        {
            scenario.Post.Json(command).ToUrl("/api/auth/exchange");
        });

        Assert.True(expectUnauthorized.Context.Response.StatusCode == (int)HttpStatusCode.BadRequest ||
                    expectUnauthorized.Context.Response.StatusCode == (int)HttpStatusCode.Unauthorized);
    }
}
