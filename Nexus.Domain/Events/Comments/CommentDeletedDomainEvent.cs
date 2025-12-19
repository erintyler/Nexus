using Nexus.Domain.Primitives;

namespace Nexus.Domain.Events.Comments;

public record CommentDeletedDomainEvent(Guid Id, Guid UserId);

