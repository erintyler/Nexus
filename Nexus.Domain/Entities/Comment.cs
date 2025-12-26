using Nexus.Domain.Common;
using Nexus.Domain.Errors;
using Nexus.Domain.Events.Comments;
using Nexus.Domain.Primitives;

namespace Nexus.Domain.Entities;

public sealed class Comment : BaseEntity
{
    public const int MaxContentLength = 2000;

    internal Comment(Guid id, Guid userId, string content) : base(id)
    {
        UserId = userId;
        Content = content;
    }

    private Comment() { }

    public Guid UserId { get; internal set; }
    public string Content { get; internal set; } = null!;

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
}