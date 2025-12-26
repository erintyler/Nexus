using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using Nexus.Application.Common.Abstractions;
using Nexus.Application.Common.Contracts;
using Nexus.Application.Common.Models;
using Nexus.Application.Common.Services;
using Nexus.Application.Features.Auth.ExchangeToken;
using Nexus.Domain.Entities;
using Nexus.Domain.Errors;
using Nexus.UnitTests.Utilities.Extensions;

namespace Nexus.Application.UnitTests.Features.Auth.ExchangeToken;

public class ExchangeTokenCommandHandlerTests
{
    private readonly Mock<IDiscordApiService> _mockDiscordApiService = new();
    private readonly Mock<IJwtTokenService> _mockJwtTokenService = new();
    private readonly Mock<IUserRepository> _mockUserRepository = new();
    private readonly Mock<ILogger<ExchangeTokenCommandHandler>> _mockLogger = new();
    private readonly Fixture _fixture = new();
    private readonly ExchangeTokenCommandHandler _handler;

    public ExchangeTokenCommandHandlerTests()
    {
        _handler = new ExchangeTokenCommandHandler(
            _mockDiscordApiService.Object,
            _mockJwtTokenService.Object,
            _mockUserRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnSuccess_WhenDiscordTokenIsValid_AndUserExists()
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

        var userId = Guid.NewGuid();
        var existingUser = new User(userId);
        var userCreatedEvent = User.Create(discordUser.Id, discordUser.Username).Value;
        existingUser.Apply(userCreatedEvent);

        var jwtToken = "jwt-token-string";
        var claims = new Dictionary<string, string>
        {
            [System.Security.Claims.ClaimTypes.NameIdentifier] = userId.ToString()
        };

        var jwtTokenResult = new JwtTokenDto(jwtToken, claims);

        _mockDiscordApiService
            .Setup(s => s.ValidateTokenAsync(command.AccessToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discordUser);

        _mockUserRepository
            .Setup(s => s.GetByDiscordIdAsync(discordUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockJwtTokenService
            .Setup(s => s.GenerateToken(userId))
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
        _mockUserRepository.Verify(s => s.GetByDiscordIdAsync(discordUser.Id, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(s => s.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockJwtTokenService.Verify(s => s.GenerateToken(userId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateUser_WhenDiscordTokenIsValid_AndUserDoesNotExist()
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

        var newUserId = Guid.NewGuid();
        var jwtToken = "jwt-token-string";
        var claims = new Dictionary<string, string>
        {
            [System.Security.Claims.ClaimTypes.NameIdentifier] = newUserId.ToString()
        };

        var jwtTokenResult = new JwtTokenDto(jwtToken, claims);

        _mockDiscordApiService
            .Setup(s => s.ValidateTokenAsync(command.AccessToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discordUser);

        _mockUserRepository
            .Setup(s => s.GetByDiscordIdAsync(discordUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockUserRepository
            .Setup(s => s.CreateAsync(discordUser.Id, discordUser.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newUserId);

        _mockJwtTokenService
            .Setup(s => s.GenerateToken(newUserId))
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
        _mockUserRepository.Verify(s => s.GetByDiscordIdAsync(discordUser.Id, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(s => s.CreateAsync(discordUser.Id, discordUser.Username, It.IsAny<CancellationToken>()), Times.Once);
        _mockJwtTokenService.Verify(s => s.GenerateToken(newUserId), Times.Once);
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
        _mockUserRepository.Verify(s => s.GetByDiscordIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockJwtTokenService.Verify(s => s.GenerateToken(It.IsAny<Guid>()), Times.Never);
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
    public async Task HandleAsync_ShouldLogInformation_WhenNewUserIsCreated()
    {
        // Arrange
        var command = new ExchangeTokenCommand("valid-discord-token");
        var discordUser = new DiscordUser
        {
            Id = "123456789",
            Username = "newuser",
            Email = "newuser@example.com"
        };

        var newUserId = Guid.NewGuid();
        var jwtToken = "jwt-token-string";
        var claims = new Dictionary<string, string>
        {
            [System.Security.Claims.ClaimTypes.NameIdentifier] = newUserId.ToString()
        };

        var jwtTokenResult = new JwtTokenDto(jwtToken, claims);

        _mockDiscordApiService
            .Setup(s => s.ValidateTokenAsync(command.AccessToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discordUser);

        _mockUserRepository
            .Setup(s => s.GetByDiscordIdAsync(discordUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockUserRepository
            .Setup(s => s.CreateAsync(discordUser.Id, discordUser.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newUserId);

        _mockJwtTokenService
            .Setup(s => s.GenerateToken(newUserId))
            .Returns(jwtTokenResult);

        // Act
        await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created new user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
