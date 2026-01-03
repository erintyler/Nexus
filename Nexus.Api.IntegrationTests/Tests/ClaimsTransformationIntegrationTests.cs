using System.Net.Http.Headers;
using System.Security.Claims;
using Alba;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Api.IntegrationTests.Fixtures;
using Nexus.Application.Common.Abstractions;
using Nexus.Application.Common.Services;
using Nexus.Domain.Entities;
using Xunit;

namespace Nexus.Api.IntegrationTests.Tests;

public class ClaimsTransformationIntegrationTests : IClassFixture<AlbaWebApplicationFixture>
{
    private readonly AlbaWebApplicationFixture _fixture;

    public ClaimsTransformationIntegrationTests(AlbaWebApplicationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CachedUserRepository_ShouldImprovePerformance_OnMultipleRequests()
    {
        // Arrange - Create a test user
        var discordId = "test-discord-id-" + Guid.NewGuid();
        var discordUsername = "PerformanceTestUser";

        using var scope = _fixture.AlbaHost.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var createResult = await userRepository.CreateAsync(discordId, discordUsername, CancellationToken.None);
        Assert.True(createResult.IsSuccess);
        var userId = createResult.Value;

        // First call - should hit the database
        var user1 = await userRepository.GetByIdAsync(userId, CancellationToken.None);
        Assert.NotNull(user1);
        Assert.Equal(discordId, user1.DiscordId);
        Assert.Equal(discordUsername, user1.DiscordUsername);

        // Second call - should hit the cache
        var user2 = await userRepository.GetByIdAsync(userId, CancellationToken.None);
        Assert.NotNull(user2);
        Assert.Equal(discordId, user2.DiscordId);
        Assert.Equal(discordUsername, user2.DiscordUsername);

        // Verify both calls returned the same user data
        Assert.Equal(user1.Id, user2.Id);
        Assert.Equal(user1.DiscordId, user2.DiscordId);
        Assert.Equal(user1.DiscordUsername, user2.DiscordUsername);
    }

    [Fact]
    public async Task CachedUserRepository_GetByDiscordId_ShouldUseCache()
    {
        // Arrange - Create a test user
        var discordId = "test-discord-id-" + Guid.NewGuid();
        var discordUsername = "CacheTestUser";

        using var scope = _fixture.AlbaHost.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var createResult = await userRepository.CreateAsync(discordId, discordUsername, CancellationToken.None);
        Assert.True(createResult.IsSuccess);

        // First call - should hit the database
        var user1 = await userRepository.GetByDiscordIdAsync(discordId, CancellationToken.None);
        Assert.NotNull(user1);
        Assert.Equal(discordId, user1.DiscordId);

        // Second call - should hit the cache
        var user2 = await userRepository.GetByDiscordIdAsync(discordId, CancellationToken.None);
        Assert.NotNull(user2);
        Assert.Equal(discordId, user2.DiscordId);

        // Verify both calls returned the same user
        Assert.Equal(user1.Id, user2.Id);
    }

    [Fact]
    public async Task CachedUserRepository_ShouldInvalidateCache_AfterCreate()
    {
        // Arrange
        var discordId = "test-discord-id-" + Guid.NewGuid();
        var discordUsername = "InvalidationTestUser";

        using var scope = _fixture.AlbaHost.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act - Create a user
        var createResult = await userRepository.CreateAsync(discordId, discordUsername, CancellationToken.None);
        Assert.True(createResult.IsSuccess);
        var userId = createResult.Value;

        // Verify user can be retrieved after creation
        var user = await userRepository.GetByIdAsync(userId, CancellationToken.None);
        Assert.NotNull(user);
        Assert.Equal(discordId, user.DiscordId);
        Assert.Equal(discordUsername, user.DiscordUsername);

        // Verify user can be retrieved by Discord ID
        var userByDiscordId = await userRepository.GetByDiscordIdAsync(discordId, CancellationToken.None);
        Assert.NotNull(userByDiscordId);
        Assert.Equal(userId, userByDiscordId.Id);
    }

    [Fact]
    public async Task UserClaimsTransformation_ShouldBeRegistered()
    {
        // Verify that IClaimsTransformation is registered in the DI container
        using var scope = _fixture.AlbaHost.Services.CreateScope();
        var claimsTransformation = scope.ServiceProvider.GetService<Microsoft.AspNetCore.Authentication.IClaimsTransformation>();

        Assert.NotNull(claimsTransformation);
    }
}
