using Nexus.Domain.Primitives;

namespace Nexus.Domain.Events.Comments;

public record CommentDeletedDomainEvent(Guid Id, Guid UserId) : INexusEvent
{
    public string EventName { get; } = "Comment deleted";
    public string Description { get; } = string.Empty;
}

