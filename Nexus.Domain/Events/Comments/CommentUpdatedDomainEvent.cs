namespace Nexus.Domain.Events.Comments;

public record CommentUpdatedDomainEvent(Guid Id, Guid UserId, string Content) : INexusEvent
{
    public string EventName { get; } = "Comment updated";
    public string Description { get; } = Content;
}