namespace Nexus.Domain.Events.Collections;

public record ImagePostAddedToCollectionDomainEvent(Guid ImagePostId) : INexusEvent
{
    public string EventName { get; } = "Image post added to collection";
    public string Description { get; } = $"ImagePostId: {ImagePostId}";
}
