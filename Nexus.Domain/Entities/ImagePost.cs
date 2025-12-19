using Nexus.Domain.Common;
using Nexus.Domain.Errors;
using Nexus.Domain.Events.Comments;
using Nexus.Domain.Events.ImagePosts;
using Nexus.Domain.Events.Tags;
using Nexus.Domain.Primitives;
using Nexus.Domain.ValueObjects;

namespace Nexus.Domain.Entities;

public class ImagePost : AggregateRoot
{
    private readonly List<Comment> _comments = [];
    private readonly List<Tag> _tags = [];
    
    private ImagePost(Guid id) : base(id)
    {
    }
    
    public string Title { get; private set; } = null!;
    
    public IReadOnlyList<Comment> Comments => _comments.AsReadOnly();
    public IReadOnlyList<Tag> Tags => _tags.AsReadOnly();
    
    public static Result<ImagePost> Create(Guid id, string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return ImagePostErrors.TitleEmpty;
        }
        
        var imagePost = new ImagePost(id);
        var createdEvent = new ImagePostCreatedDomainEvent(title);
        
        imagePost.RaiseDomainEvent(createdEvent);
        imagePost.Apply(createdEvent);
        return imagePost;
    }
    
    public void Apply(ImagePostCreatedDomainEvent @event)
    {
        Title = @event.Title;
    }
    
    public Result AddComment(Guid commentId, Guid userId, string content)
    {
        var commentResult = Comment.Create(commentId, userId, content);
        if (commentResult.IsFailure)
        {
            return commentResult.Error;
        }
        
        var comment = commentResult.Value;
        var commentCreated = new CommentCreatedDomainEvent(comment.Id, comment.UserId, comment.Content);
        
        RaiseDomainEvent(commentCreated);
        Apply(commentCreated);
        return Result.Success();
    }
    
    public void Apply(CommentCreatedDomainEvent @event)
    {
        var comment = Comment.Create(@event.CommentId, @event.UserId, @event.Content);
        
        if (comment.IsFailure)
        {
            throw new InvalidOperationException($"Failed to apply CommentCreated event: {comment.Error.Code}");
        }
        
        _comments.Add(comment.Value);
    }
    
    public Result UpdateComment(Guid commentId, Guid userId, string newContent)
    {
        var comment = _comments.FirstOrDefault(c => c.Id == commentId);
        if (comment is null)
        {
            return CommentErrors.NotFound;
        }
        
        var updateResult = comment.UpdateContent(userId, newContent);
        if (updateResult.IsFailure)
        {
            return updateResult.Error;
        }
        
        var commentUpdated = updateResult.Value;
        
        RaiseDomainEvent(commentUpdated);
        Apply(commentUpdated);
        return Result.Success();
    }
    
    public void Apply(CommentUpdatedDomainEvent @event) 
    {
        var comment = _comments.FirstOrDefault(c => c.Id == @event.CommentId);
        if (comment is null)
        {
            throw new InvalidOperationException("Failed to apply CommentUpdated event: Comment not found.");
        }
        
        comment.Apply(@event);
    }

    public Result DeleteComment(Guid commentId, Guid userId)
    {
        var comment = _comments.FirstOrDefault(c => c.Id == commentId);
        if (comment is null)
        {
            return CommentErrors.NotFound;
        }
        
        if (comment.UserId != userId)
        {
            return CommentErrors.NotAuthor;
        }
        
        var commentDeleted = new CommentDeletedDomainEvent(comment.Id, userId);
       
        RaiseDomainEvent(commentDeleted);
        Apply(commentDeleted);
        return Result.Success();
    }
    
    public void Apply(CommentDeletedDomainEvent @event)
    {
        var comment = _comments.FirstOrDefault(c => c.Id == @event.CommentId);
        if (comment is null)
        {
            throw new InvalidOperationException("Failed to apply CommentDeleted event: Comment not found.");
        }
        
        _comments.Remove(comment);
    }

    public Result AddTag(Tag tag)
    {
        if (_tags.Contains(tag))
        {
            return TagErrors.AlreadyExists;
        }
        
        var tagAdded = new TagAddedDomainEvent(tag.Value, tag.Type);
        
        RaiseDomainEvent(tagAdded);
        Apply(tagAdded);
        return Result.Success();
    }
    
    public void Apply(TagAddedDomainEvent @event)
    {
        var tag = Tag.Create(@event.TagValue, @event.TagType);
        
        if (tag.IsFailure)
        {
            throw new InvalidOperationException($"Failed to apply TagAdded event: {tag.Error.Code}");
        }
        
        _tags.Add(tag.Value);
    }
}