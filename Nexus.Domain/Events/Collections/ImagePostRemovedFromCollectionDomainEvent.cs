namespace Nexus.Domain.Events.Collections;

public record ImagePostRemovedFromCollectionDomainEvent(Guid ImagePostId) : INexusEvent
{
    public string EventName { get; } = "Image post removed from collection";
    public string Description { get; } = $"ImagePostId: {ImagePostId}";
}
