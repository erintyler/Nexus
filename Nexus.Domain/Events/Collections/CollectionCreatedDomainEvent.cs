namespace Nexus.Domain.Events.Collections;

public record CollectionCreatedDomainEvent(Guid UserId, string Title) : INexusEvent
{
    public string EventName { get; } = "Collection created";
    public string Description { get; } = $"User: {UserId} | Title: {Title}";
}
