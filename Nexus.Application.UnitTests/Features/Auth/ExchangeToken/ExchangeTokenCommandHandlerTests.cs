using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using Nexus.Application.Common.Contracts;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Services;
using Nexus.Application.Features.Auth.ExchangeToken;
using Nexus.Domain.Errors;
using Nexus.UnitTests.Utilities.Extensions;

namespace Nexus.Application.UnitTests.Features.Auth.ExchangeToken;

public class ExchangeTokenCommandHandlerTests
{
    private readonly Mock<IDiscordApiService> _mockDiscordApiService = new();
    private readonly Mock<IJwtTokenService> _mockJwtTokenService = new();
    private readonly Mock<ILogger<ExchangeTokenCommandHandler>> _mockLogger = new();
    private readonly Fixture _fixture = new();
    private readonly ExchangeTokenCommandHandler _handler;

    public ExchangeTokenCommandHandlerTests()
    {
        _handler = new ExchangeTokenCommandHandler(
            _mockDiscordApiService.Object,
            _mockJwtTokenService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnSuccess_WhenDiscordTokenIsValid()
    {
        // Arrange
        var command = new ExchangeTokenCommand("valid-discord-token");
        var discordUser = new DiscordUser
        {
            Id = "123456789",
            Username = "testuser",
            Email = "test@example.com",
            Discriminator = "0001",
            Avatar = "avatar-hash"
        };

        var jwtToken = "jwt-token-string";
        var claims = new Dictionary<string, string>
        {
            ["discord_id"] = discordUser.Id,
            ["discord_username"] = discordUser.Username
        };

        var jwtTokenResult = new JwtTokenResult(jwtToken, claims);

        _mockDiscordApiService
            .Setup(s => s.ValidateTokenAsync(command.AccessToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discordUser);

        _mockJwtTokenService
            .Setup(s => s.GenerateToken(discordUser))
            .Returns(jwtTokenResult);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(jwtToken, result.Value.AccessToken);
        Assert.Equal("Bearer", result.Value.TokenType);
        Assert.Equal(3600, result.Value.ExpiresIn);
        Assert.Equal(claims, result.Value.Claims);

        _mockDiscordApiService.Verify(s => s.ValidateTokenAsync(command.AccessToken, It.IsAny<CancellationToken>()), Times.Once);
        _mockJwtTokenService.Verify(s => s.GenerateToken(discordUser), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnInvalidTokenError_WhenDiscordTokenIsInvalid()
    {
        // Arrange
        var command = new ExchangeTokenCommand("invalid-discord-token");

        _mockDiscordApiService
            .Setup(s => s.ValidateTokenAsync(command.AccessToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscordUser?)null);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == AuthErrors.InvalidToken.Code);

        _mockDiscordApiService.Verify(s => s.ValidateTokenAsync(command.AccessToken, It.IsAny<CancellationToken>()), Times.Once);
        _mockJwtTokenService.Verify(s => s.GenerateToken(It.IsAny<DiscordUser>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldLogWarning_WhenDiscordTokenValidationFails()
    {
        // Arrange
        var command = new ExchangeTokenCommand("invalid-discord-token");

        _mockDiscordApiService
            .Setup(s => s.ValidateTokenAsync(command.AccessToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscordUser?)null);

        // Act
        await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to validate Discord token")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCompleteUserInfo_WhenUserHasAllFields()
    {
        // Arrange
        var command = new ExchangeTokenCommand("valid-discord-token");
        var discordUser = new DiscordUser
        {
            Id = "987654321",
            Username = "fulluser",
            Email = "fulluser@example.com",
            Discriminator = "9999",
            Avatar = "full-avatar-hash"
        };

        var jwtToken = "jwt-token-string";
        var claims = new Dictionary<string, string>
        {
            ["discord_id"] = discordUser.Id,
            ["discord_username"] = discordUser.Username,
            ["discord_discriminator"] = discordUser.Discriminator,
            ["discord_avatar"] = discordUser.Avatar
        };

        var jwtTokenResult = new JwtTokenResult(jwtToken, claims);

        _mockDiscordApiService
            .Setup(s => s.ValidateTokenAsync(command.AccessToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discordUser);

        _mockJwtTokenService
            .Setup(s => s.GenerateToken(discordUser))
            .Returns(jwtTokenResult);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(jwtToken, result.Value.AccessToken);
        Assert.Equal("Bearer", result.Value.TokenType);
        Assert.Equal(3600, result.Value.ExpiresIn);
        Assert.NotEmpty(result.Value.Claims);
        Assert.Contains(claims, c => result.Value.Claims.Contains(c));
    }

    [Fact]
    public async Task HandleAsync_ShouldHandleNullOptionalFields_WhenUserHasMinimalData()
    {
        // Arrange
        var command = new ExchangeTokenCommand("valid-discord-token");
        var discordUser = new DiscordUser
        {
            Id = "111111111",
            Username = "minimaluser",
            Email = null,
            Discriminator = null,
            Avatar = null
        };

        var jwtToken = "jwt-token-string";
        var claims = new Dictionary<string, string>
        {
            ["discord_id"] = discordUser.Id,
            ["discord_username"] = discordUser.Username
        };

        var jwtTokenResult = new JwtTokenResult(jwtToken, claims);

        _mockDiscordApiService
            .Setup(s => s.ValidateTokenAsync(command.AccessToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discordUser);

        _mockJwtTokenService
            .Setup(s => s.GenerateToken(discordUser))
            .Returns(jwtTokenResult);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(jwtToken, result.Value.AccessToken);
        Assert.Equal("Bearer", result.Value.TokenType);
        Assert.Equal(3600, result.Value.ExpiresIn);
        Assert.NotEmpty(result.Value.Claims);
    }
}
