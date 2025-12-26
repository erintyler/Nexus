namespace Nexus.Application.Common.Contracts;

public record DiscordUser
{
    public required string Id { get; init; }
    public required string Username { get; init; }
    public string? Discriminator { get; init; }
    public string? Avatar { get; init; }
    public string? Email { get; init; }
}
