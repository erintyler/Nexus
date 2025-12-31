using Nexus.Application.Common.Abstractions;
using Nexus.Domain.Common;
using Nexus.Domain.Entities;
using ZiggyCreatures.Caching.Fusion;

namespace Nexus.Infrastructure.Repositories;

/// <summary>
/// Decorator for IUserRepository that adds caching using FusionCache.
/// Implements the decorator pattern to add caching behavior to user repository operations.
/// </summary>
public class CachedUserRepository(
    IUserRepository innerRepository,
    IFusionCache cache) : IUserRepository
{
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);
    private const string CacheKeyPrefixById = "user:id:";
    private const string CacheKeyPrefixByDiscordId = "user:discord:";

    public async Task<User?> GetByDiscordIdAsync(string discordId, CancellationToken ct)
    {
        var cacheKey = $"{CacheKeyPrefixByDiscordId}{discordId}";

        return await cache.GetOrSetAsync(
            cacheKey,
            async _ => await innerRepository.GetByDiscordIdAsync(discordId, ct),
            options => options.SetDuration(DefaultCacheDuration),
            ct);
    }

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken ct)
    {
        var cacheKey = $"{CacheKeyPrefixById}{userId}";

        return await cache.GetOrSetAsync(
            cacheKey,
            async _ => await innerRepository.GetByIdAsync(userId, ct),
            options => options.SetDuration(DefaultCacheDuration),
            ct);
    }

    public async Task<Result<Guid>> CreateAsync(string discordId, string discordUsername, CancellationToken ct)
    {
        var result = await innerRepository.CreateAsync(discordId, discordUsername, ct);

        // If creation was successful, invalidate related cache entries
        if (result.IsSuccess)
        {
            var cacheKeyByDiscordId = $"{CacheKeyPrefixByDiscordId}{discordId}";
            var cacheKeyById = $"{CacheKeyPrefixById}{result.Value}";

            await cache.RemoveAsync(cacheKeyByDiscordId, token: ct);
            await cache.RemoveAsync(cacheKeyById, token: ct);
        }

        return result;
    }
}
