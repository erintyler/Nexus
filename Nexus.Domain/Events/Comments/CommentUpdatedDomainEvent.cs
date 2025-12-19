using Nexus.Domain.Primitives;

namespace Nexus.Domain.Events.Comments;

public record CommentUpdatedDomainEvent(Guid Id, Guid UserId, string Content);

