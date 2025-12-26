namespace Nexus.Domain.Events.Users;

public record UserCreatedDomainEvent(string DiscordId, string DiscordUsername) : INexusEvent
{
    public string EventName { get; } = "User created";
    public string Description { get; } = $"Discord ID: {DiscordId} | Username: {DiscordUsername}";
}
