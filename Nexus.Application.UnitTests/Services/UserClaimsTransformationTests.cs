using System.Security.Claims;
using AutoFixture;
using Moq;
using Nexus.Api.Services;
using Nexus.Application.Common.Abstractions;
using Nexus.Domain.Entities;
using Nexus.UnitTests.Utilities.Extensions;

namespace Nexus.Application.UnitTests.Services;

public class UserClaimsTransformationTests
{
    private readonly Mock<IUserRepository> _mockUserRepository = new();
    private readonly UserClaimsTransformation _transformation;
    private readonly Fixture _fixture = new();

    public UserClaimsTransformationTests()
    {
        _transformation = new UserClaimsTransformation(_mockUserRepository.Object);
    }

    [Fact]
    public async Task TransformAsync_ShouldAddDiscordClaims_WhenUserExists()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var discordId = _fixture.Create<string>();
        var discordUsername = _fixture.Create<string>();

        var user = new User(userId);
        var userCreatedEvent = User.Create(discordId, discordUsername).Value;
        user.Apply(userCreatedEvent);

        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
        var principal = new ClaimsPrincipal(identity);

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId.ToString(), result.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal(discordId, result.FindFirst("discord_id")?.Value);
        Assert.Equal(discordUsername, result.FindFirst("discord_username")?.Value);
        Assert.NotNull(result.FindFirst("transformed"));

        _mockUserRepository.Verify(
            r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TransformAsync_ShouldReturnOriginalPrincipal_WhenUserNotFound()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();

        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
        var principal = new ClaimsPrincipal(identity);

        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId.ToString(), result.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Null(result.FindFirst("discord_id"));
        Assert.Null(result.FindFirst("discord_username"));

        _mockUserRepository.Verify(
            r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TransformAsync_ShouldReturnOriginalPrincipal_WhenNoNameIdentifierClaim()
    {
        // Arrange
        var identity = new ClaimsIdentity("Test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.FindFirst("discord_id"));
        Assert.Null(result.FindFirst("discord_username"));

        _mockUserRepository.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TransformAsync_ShouldReturnOriginalPrincipal_WhenUserIdIsInvalid()
    {
        // Arrange
        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "invalid-guid"));
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.FindFirst("discord_id"));
        Assert.Null(result.FindFirst("discord_username"));

        _mockUserRepository.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TransformAsync_ShouldNotCallRepository_WhenAlreadyTransformed()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
        identity.AddClaim(new Claim("transformed", "true"));
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("true", result.FindFirst("transformed")?.Value);

        // Repository should not be called since transformation already applied
        _mockUserRepository.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
