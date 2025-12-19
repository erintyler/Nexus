using Nexus.Domain.Primitives;

namespace Nexus.Domain.Events.Comments;

public record CommentCreatedDomainEvent(Guid Id, Guid UserId, string Content);
