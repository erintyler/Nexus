using Nexus.Domain.Primitives;

namespace Nexus.Domain.Events.Comments;

public record CommentDeletedDomainEvent(Guid CommentId, Guid UserId) : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
}

