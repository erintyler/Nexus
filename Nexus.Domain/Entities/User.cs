using System.Text.Json.Serialization;
using Nexus.Domain.Common;
using Nexus.Domain.Errors;
using Nexus.Domain.Events.Users;
using Nexus.Domain.Primitives;

namespace Nexus.Domain.Entities;

/// <summary>
/// User aggregate root - represents an internal user linked to a Discord user.
/// This entity follows event sourcing and is reconstructed from domain events.
/// </summary>
public sealed class User : BaseEntity
{
    internal User() { }

    [JsonConstructor]
    internal User(Guid id) : base(id) { }

    // Properties with private setters for encapsulation
    public string DiscordId { get; private set; } = null!;
    public string DiscordUsername { get; private set; } = null!;

    // Marten event sourcing metadata
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastModified { get; private set; }
    public string LastModifiedBy { get; private set; } = null!;

    /// <summary>
    /// Factory method to create a new User with validation and business rules.
    /// Returns the creation event if successful.
    /// </summary>
    public static Result<UserCreatedDomainEvent> Create(string discordId, string discordUsername)
    {
        // Validate discord ID
        if (string.IsNullOrWhiteSpace(discordId))
        {
            return UserErrors.DiscordIdEmpty;
        }

        // Validate discord username
        if (string.IsNullOrWhiteSpace(discordUsername))
        {
            return UserErrors.DiscordUsernameEmpty;
        }

        return new UserCreatedDomainEvent(discordId, discordUsername);
    }

    // Event application methods for event sourcing
    // Note: No validation in Apply methods - events represent historical facts that were already validated
    // when created. During reconstruction, we trust the event stream.
    public void Apply(UserCreatedDomainEvent @event)
    {
        DiscordId = @event.DiscordId;
        DiscordUsername = @event.DiscordUsername;
    }
}
