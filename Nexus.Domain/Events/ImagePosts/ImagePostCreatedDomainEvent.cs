using Nexus.Domain.Primitives;

namespace Nexus.Domain.Events.ImagePosts;

public record ImagePostCreatedDomainEvent(string Title) : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
}
