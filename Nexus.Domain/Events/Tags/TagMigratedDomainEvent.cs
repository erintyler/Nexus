using Nexus.Domain.Primitives;

namespace Nexus.Domain.Events.Tags;

public record TagMigratedDomainEvent(Guid UserId, TagData Source, TagData Target) : INexusEvent
{
    public string EventName { get; } = "Tag migrated";
    public string Description { get; } = $"User: {UserId} | From: {Source.Type}:{Source.Value} To: {Target.Type}:{Target.Value}";
}