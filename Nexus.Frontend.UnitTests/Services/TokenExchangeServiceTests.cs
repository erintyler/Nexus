using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Nexus.Application.Common.Contracts;
using Nexus.Frontend.Services;
using RichardSzalay.MockHttp;

namespace Nexus.Frontend.UnitTests.Services;

public class TokenExchangeServiceTests
{
    private readonly Mock<ILogger<TokenExchangeService>> _loggerMock;
    private readonly MockHttpMessageHandler _mockHttp;
    private const string ApiBaseUrl = "https://nexus-api";
    private const string ExchangeEndpoint = "/api/auth/exchange";

    public TokenExchangeServiceTests()
    {
        _loggerMock = new Mock<ILogger<TokenExchangeService>>();
        _mockHttp = new MockHttpMessageHandler();
    }

    private HttpClient CreateHttpClient()
    {
        var client = _mockHttp.ToHttpClient();
        client.BaseAddress = new Uri(ApiBaseUrl);
        return client;
    }

    [Fact]
    public async Task ExchangeTokenAsync_ShouldReturnTokenExchangeResponse_WhenApiReturnsSuccess()
    {
        // Arrange
        var discordAccessToken = "discord-access-token-123";
        var expectedResponse = new TokenExchangeResponse
        {
            AccessToken = "jwt-token-xyz",
            TokenType = "Bearer",
            ExpiresIn = 3600,
            Claims = new Dictionary<string, string>
            {
                ["discord_id"] = "123456789",
                ["discord_username"] = "testuser"
            }
        };

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };

        _mockHttp
            .Expect(HttpMethod.Post, $"{ApiBaseUrl}{ExchangeEndpoint}")
            .WithContent("{\"accessToken\":\"discord-access-token-123\"}")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(expectedResponse, options));

        var httpClient = CreateHttpClient();
        var service = new TokenExchangeService(httpClient, _loggerMock.Object);

        // Act
        var result = await service.ExchangeTokenAsync(discordAccessToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.AccessToken, result.AccessToken);
        Assert.Equal(expectedResponse.TokenType, result.TokenType);
        Assert.Equal(expectedResponse.ExpiresIn, result.ExpiresIn);
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ExchangeTokenAsync_ShouldReturnNull_WhenApiReturnsUnauthorized()
    {
        // Arrange
        var discordAccessToken = "invalid-discord-token";

        _mockHttp
            .Expect(HttpMethod.Post, $"{ApiBaseUrl}{ExchangeEndpoint}")
            .Respond(HttpStatusCode.Unauthorized);

        var httpClient = CreateHttpClient();
        var service = new TokenExchangeService(httpClient, _loggerMock.Object);

        // Act
        var result = await service.ExchangeTokenAsync(discordAccessToken);

        // Assert
        Assert.Null(result);
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ExchangeTokenAsync_ShouldReturnNull_WhenApiReturnsBadRequest()
    {
        // Arrange
        var discordAccessToken = "malformed-token";

        _mockHttp
            .Expect(HttpMethod.Post, $"{ApiBaseUrl}{ExchangeEndpoint}")
            .Respond(HttpStatusCode.BadRequest);

        var httpClient = CreateHttpClient();
        var service = new TokenExchangeService(httpClient, _loggerMock.Object);

        // Act
        var result = await service.ExchangeTokenAsync(discordAccessToken);

        // Assert
        Assert.Null(result);
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ExchangeTokenAsync_ShouldReturnNull_WhenApiReturnsServerError()
    {
        // Arrange
        var discordAccessToken = "discord-token";

        _mockHttp
            .Expect(HttpMethod.Post, $"{ApiBaseUrl}{ExchangeEndpoint}")
            .Respond(HttpStatusCode.InternalServerError);

        var httpClient = CreateHttpClient();
        var service = new TokenExchangeService(httpClient, _loggerMock.Object);

        // Act
        var result = await service.ExchangeTokenAsync(discordAccessToken);

        // Assert
        Assert.Null(result);
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ExchangeTokenAsync_ShouldReturnNull_WhenHttpClientThrowsException()
    {
        // Arrange
        var discordAccessToken = "discord-token";

        _mockHttp
            .Expect(HttpMethod.Post, $"{ApiBaseUrl}{ExchangeEndpoint}")
            .Throw(new HttpRequestException("Network error"));

        var httpClient = CreateHttpClient();
        var service = new TokenExchangeService(httpClient, _loggerMock.Object);

        // Act
        var result = await service.ExchangeTokenAsync(discordAccessToken);

        // Assert
        Assert.Null(result);
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ExchangeTokenAsync_ShouldSendCorrectRequestBody_WhenCalled()
    {
        // Arrange
        var discordAccessToken = "test-discord-token";
        var expectedResponse = new TokenExchangeResponse
        {
            AccessToken = "jwt",
            TokenType = "Bearer",
            ExpiresIn = 3600,
            Claims = new Dictionary<string, string>
            {
                ["discord_id"] = "123456789",
                ["discord_username"] = "testuser"
            }
        };

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };

        _mockHttp
            .Expect(HttpMethod.Post, $"{ApiBaseUrl}{ExchangeEndpoint}")
            .WithContent($"{{\"accessToken\":\"{discordAccessToken}\"}}")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(expectedResponse, options));

        var httpClient = CreateHttpClient();
        var service = new TokenExchangeService(httpClient, _loggerMock.Object);

        // Act
        var result = await service.ExchangeTokenAsync(discordAccessToken);

        // Assert
        Assert.NotNull(result);
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ExchangeTokenAsync_ShouldLogWarning_WhenApiReturnsNonSuccessStatusCode()
    {
        // Arrange
        var discordAccessToken = "discord-token";

        _mockHttp
            .Expect(HttpMethod.Post, $"{ApiBaseUrl}{ExchangeEndpoint}")
            .Respond(HttpStatusCode.Unauthorized);

        var httpClient = CreateHttpClient();
        var service = new TokenExchangeService(httpClient, _loggerMock.Object);

        // Act
        await service.ExchangeTokenAsync(discordAccessToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Token exchange failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExchangeTokenAsync_ShouldLogError_WhenExceptionIsThrown()
    {
        // Arrange
        var discordAccessToken = "discord-token";

        _mockHttp
            .Expect(HttpMethod.Post, $"{ApiBaseUrl}{ExchangeEndpoint}")
            .Throw(new HttpRequestException("Network error"));

        var httpClient = CreateHttpClient();
        var service = new TokenExchangeService(httpClient, _loggerMock.Object);

        // Act
        await service.ExchangeTokenAsync(discordAccessToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error exchanging token")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("short-token")]
    [InlineData("very-long-token-with-many-characters-1234567890")]
    [InlineData("token.with.dots")]
    [InlineData("token_with_underscores")]
    public async Task ExchangeTokenAsync_ShouldHandleDifferentTokenFormats(string discordAccessToken)
    {
        // Arrange
        var expectedResponse = new TokenExchangeResponse
        {
            AccessToken = "jwt-token",
            TokenType = "Bearer",
            ExpiresIn = 3600,
            Claims = new Dictionary<string, string>
            {
                ["discord_id"] = "123456789",
                ["discord_username"] = "testuser"
            }
        };

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };

        _mockHttp
            .Expect(HttpMethod.Post, $"{ApiBaseUrl}{ExchangeEndpoint}")
            .Respond("application/json", System.Text.Json.JsonSerializer.Serialize(expectedResponse, options));

        var httpClient = CreateHttpClient();
        var service = new TokenExchangeService(httpClient, _loggerMock.Object);

        // Act
        var result = await service.ExchangeTokenAsync(discordAccessToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.AccessToken, result.AccessToken);
        _mockHttp.VerifyNoOutstandingExpectation();
    }
}

