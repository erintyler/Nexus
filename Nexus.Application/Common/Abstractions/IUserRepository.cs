using Nexus.Domain.Entities;

namespace Nexus.Application.Common.Abstractions;

/// <summary>
/// Repository abstraction for User operations.
/// Provides data access methods for user management workflows.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Get a user by their Discord ID.
    /// </summary>
    Task<User?> GetByDiscordIdAsync(string discordId, CancellationToken ct);

    /// <summary>
    /// Get a user by their internal user ID.
    /// </summary>
    Task<User?> GetByIdAsync(Guid userId, CancellationToken ct);

    /// <summary>
    /// Create a new user from events.
    /// </summary>
    Task<Guid> CreateAsync(string discordId, string discordUsername, CancellationToken ct);
}
