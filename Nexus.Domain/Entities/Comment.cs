using Nexus.Domain.Common;
using Nexus.Domain.Errors;
using Nexus.Domain.Events.Comments;
using Nexus.Domain.Primitives;

namespace Nexus.Domain.Entities;

public class Comment : Entity
{
    public const int MaxContentLength = 2000;

    private Comment(Guid id, Guid userId, string content) : base(id)
    {
        UserId = userId;
        Content = content;
    }
    
    private Comment()
    {
    }
    
    public Guid UserId { get; private init; }
    public string Content { get; private set; } = null!;
    
    public static Result<Comment> Create(Guid id, Guid userId, string content)
    {
        if (userId == Guid.Empty)
        {
            return CommentErrors.UserIdEmpty;
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return CommentErrors.ContentEmpty;
        }

        if (content.Length > MaxContentLength)
        {
            return CommentErrors.ContentTooLong;
        }

        return new Comment(id, userId, content);
    }
    
    internal Result<CommentUpdatedDomainEvent> UpdateContent(Guid userId, string newContent)
    {
        if (userId != UserId)
        {
            return CommentErrors.NotAuthor;
        }

        if (string.IsNullOrWhiteSpace(newContent))
        {
            return CommentErrors.ContentEmpty;
        }

        if (newContent.Length > MaxContentLength)
        {
            return CommentErrors.ContentTooLong;
        }

        return new CommentUpdatedDomainEvent(Id, userId, newContent);
    }
    
    internal void Apply(CommentUpdatedDomainEvent @event)
    {
        Content = @event.Content;
    }
}