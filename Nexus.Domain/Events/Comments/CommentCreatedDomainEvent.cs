namespace Nexus.Domain.Events.Comments;

public record CommentCreatedDomainEvent(Guid Id, Guid UserId, string Content) : INexusEvent
{
    public string EventName { get; } = "Comment created";
    public string Description { get; } = Content;
}
