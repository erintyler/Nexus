using AutoFixture;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Nexus.Application.Common.Abstractions;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using Nexus.Infrastructure.Repositories;
using Nexus.UnitTests.Utilities.Extensions;
using ZiggyCreatures.Caching.Fusion;

namespace Nexus.Application.UnitTests.Infrastructure.Repositories;

public class CachedUserRepositoryTests
{
    private readonly Mock<IUserRepository> _mockInnerRepository = new();
    private readonly IFusionCache _cache;
    private readonly CachedUserRepository _cachedRepository;
    private readonly Fixture _fixture = new();

    public CachedUserRepositoryTests()
    {
        // Create a real FusionCache instance for testing
        _cache = new FusionCache(new FusionCacheOptions());
        _cachedRepository = new CachedUserRepository(_mockInnerRepository.Object, _cache);
    }

    [Fact]
    public async Task GetByDiscordIdAsync_ShouldReturnUserFromCache_WhenCalledTwice()
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        var userId = _fixture.Create<Guid>();
        var user = new User(userId);
        var userCreatedEvent = User.Create(discordId, _fixture.Create<string>()).Value;
        user.Apply(userCreatedEvent);

        _mockInnerRepository
            .Setup(r => r.GetByDiscordIdAsync(discordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result1 = await _cachedRepository.GetByDiscordIdAsync(discordId, TestContext.Current.CancellationToken);
        var result2 = await _cachedRepository.GetByDiscordIdAsync(discordId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(user.Id, result1.Id);
        Assert.Equal(user.Id, result2.Id);

        // Verify inner repository was only called once (second call used cache)
        _mockInnerRepository.Verify(
            r => r.GetByDiscordIdAsync(discordId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUserFromCache_WhenCalledTwice()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();
        var discordId = _fixture.Create<string>();
        var user = new User(userId);
        var userCreatedEvent = User.Create(discordId, _fixture.Create<string>()).Value;
        user.Apply(userCreatedEvent);

        _mockInnerRepository
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result1 = await _cachedRepository.GetByIdAsync(userId, TestContext.Current.CancellationToken);
        var result2 = await _cachedRepository.GetByIdAsync(userId, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.Equal(userId, result1.Id);
        Assert.Equal(userId, result2.Id);

        // Verify inner repository was only called once (second call used cache)
        _mockInnerRepository.Verify(
            r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByDiscordIdAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        var discordId = _fixture.Create<string>();

        _mockInnerRepository
            .Setup(r => r.GetByDiscordIdAsync(discordId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _cachedRepository.GetByDiscordIdAsync(discordId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
        _mockInnerRepository.Verify(
            r => r.GetByDiscordIdAsync(discordId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        var userId = _fixture.Create<Guid>();

        _mockInnerRepository
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _cachedRepository.GetByIdAsync(userId, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result);
        _mockInnerRepository.Verify(
            r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldInvalidateCache_WhenSuccessful()
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        var discordUsername = _fixture.Create<string>();
        var newUserId = _fixture.Create<Guid>();

        _mockInnerRepository
            .Setup(r => r.CreateAsync(discordId, discordUsername, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(newUserId));

        // Pre-populate cache with dummy data
        var dummyUser = new User(newUserId);
        var dummyEvent = User.Create(discordId, discordUsername).Value;
        dummyUser.Apply(dummyEvent);

        await _cache.SetAsync($"user:discord:{discordId}", dummyUser);
        await _cache.SetAsync($"user:id:{newUserId}", dummyUser);

        // Act
        var result = await _cachedRepository.CreateAsync(discordId, discordUsername, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(newUserId, result.Value);

        // Verify cache was invalidated
        var cachedByDiscordId = await _cache.TryGetAsync<User>($"user:discord:{discordId}");
        var cachedById = await _cache.TryGetAsync<User>($"user:id:{newUserId}");

        Assert.False(cachedByDiscordId.HasValue);
        Assert.False(cachedById.HasValue);

        _mockInnerRepository.Verify(
            r => r.CreateAsync(discordId, discordUsername, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldNotInvalidateCache_WhenFailed()
    {
        // Arrange
        var discordId = _fixture.Create<string>();
        var discordUsername = _fixture.Create<string>();
        var error = new Error("TEST_ERROR", ErrorType.Validation, "Test error");

        _mockInnerRepository
            .Setup(r => r.CreateAsync(discordId, discordUsername, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<Guid>(error));

        // Pre-populate cache with dummy data
        var dummyUserId = _fixture.Create<Guid>();
        var dummyUser = new User(dummyUserId);
        var dummyEvent = User.Create(discordId, discordUsername).Value;
        dummyUser.Apply(dummyEvent);

        await _cache.SetAsync($"user:discord:{discordId}", dummyUser);

        // Act
        var result = await _cachedRepository.CreateAsync(discordId, discordUsername, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);

        // Verify cache was NOT invalidated
        var cachedByDiscordId = await _cache.TryGetAsync<User>($"user:discord:{discordId}");
        Assert.True(cachedByDiscordId.HasValue);

        _mockInnerRepository.Verify(
            r => r.CreateAsync(discordId, discordUsername, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
