using Nexus.Domain.Primitives;

namespace Nexus.Domain.Events.Comments;

public record CommentUpdatedDomainEvent(Guid CommentId, Guid UserId, string Content) : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
}

