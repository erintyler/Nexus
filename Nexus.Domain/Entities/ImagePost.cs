using Nexus.Domain.Abstractions;
using Nexus.Domain.Common;
using Nexus.Domain.Errors;
using Nexus.Domain.Events.Comments;
using Nexus.Domain.Events.ImagePosts;
using Nexus.Domain.Events.Tags;
using Nexus.Domain.Extensions;
using Nexus.Domain.Primitives;
using Nexus.Domain.ValueObjects;

namespace Nexus.Domain.Entities;

/// <summary>
/// ImagePost aggregate root - represents the domain model for an image post.
/// This entity follows event sourcing and is reconstructed from domain events.
/// Encapsulates business rules and generates domain events.
/// </summary>
public class ImagePost : ITaggable
{
    public const int MinTitleLength = 1;
    public const int MaxTitleLength = 200;
    
    private readonly List<Comment> _comments = [];
    private readonly HashSet<Tag> _tags = [];
    
    // Private constructor for event sourcing reconstruction
    private ImagePost() { }
    
    // Properties with private setters for encapsulation
    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = null!;
    public DateTimeOffset LastModified { get; private set; }
    public string LastModifiedBy { get; private set; } = null!;
    
    // Expose collections as read-only
    public IReadOnlyList<Comment> Comments => _comments.AsReadOnly();
    public IReadOnlySet<Tag> Tags => _tags.AsReadOnly();
    
    /// <summary>
    /// Factory method to create a new ImagePost with validation and business rules.
    /// Returns the creation event if successful.
    /// </summary>
    public static Result<ImagePostCreatedDomainEvent> Create(Guid userId, string title, IReadOnlyList<Tag> tags)
    {
        // Validate userId
        if (userId == Guid.Empty)
        {
            return ImagePostErrors.UserIdEmpty;
        }
        
        // Validate title
        if (string.IsNullOrWhiteSpace(title))
        {
            return ImagePostErrors.TitleEmpty;
        }
        
        if (title.Length > MaxTitleLength)
        {
            return ImagePostErrors.TitleTooLong;
        }
        
        // Validate tags
        var tagResults = tags
            .Select(t => Tag.Create(t.Value, t.Type))
            .ToList();

        var errors = tagResults
            .WithIndexedErrors("tags")
            .ToList();
        
        if (errors.Count != 0)
        {
            return Result.Failure<ImagePostCreatedDomainEvent>(errors);
        }
        
        var validatedTags = tagResults.Select(tr => tr.Value).ToList();
        
        // Create and return the domain event
        return new ImagePostCreatedDomainEvent(userId, title, validatedTags);
    }
    
    /// <summary>
    /// Add tags to the image post, ensuring no duplicates.
    /// Returns events for only the new tags.
    /// </summary>
    public Result<IEnumerable<TagAddedDomainEvent>> AddTags(IReadOnlyList<Tag> newTags)
    {
        // Validate tags
        var tagResults = newTags
            .Select(t => Tag.Create(t.Value, t.Type))
            .ToList();

        var errors = tagResults
            .WithIndexedErrors("tags")
            .ToList();
        
        if (errors.Count != 0)
        {
            return Result.Failure<IEnumerable<TagAddedDomainEvent>>(errors);
        }
        
        var validatedTags = tagResults.Select(tr => tr.Value).ToList();
        
        // Only create events for tags that don't already exist
        var events = validatedTags
            .Except(_tags)
            .Select(t => new TagAddedDomainEvent(t.Value, t.Type));
        
        return Result.Success(events);
    }
    
    /// <summary>
    /// Add a comment to the image post with validation.
    /// </summary>
    public Result<CommentCreatedDomainEvent> AddComment(Guid commentId, Guid userId, string content)
    {
        var commentResult = Comment.Create(commentId, userId, content);
        
        if (commentResult.IsFailure)
        {
            return Result.Failure<CommentCreatedDomainEvent>(commentResult.Errors);
        }
        
        return new CommentCreatedDomainEvent(commentId, userId, content);
    }
    
    /// <summary>
    /// Update a comment with validation and authorization.
    /// </summary>
    public Result<CommentUpdatedDomainEvent> UpdateComment(Guid commentId, Guid userId, string newContent)
    {
        var comment = _comments.FirstOrDefault(c => c.Id == commentId);
        
        if (comment is null)
        {
            return CommentErrors.NotFound;
        }
        
        return comment.UpdateContent(userId, newContent);
    }
    
    /// <summary>
    /// Delete a comment with authorization check.
    /// </summary>
    public Result<CommentDeletedDomainEvent> DeleteComment(Guid commentId, Guid userId)
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
        
        return new CommentDeletedDomainEvent(commentId, userId);
    }
    
    // Event application methods for event sourcing
    public void Apply(ImagePostCreatedDomainEvent @event)
    {
        Title = @event.Title;
        CreatedBy = @event.UserId.ToString();
        
        foreach (var tag in @event.Tags)
        {
            _tags.Add(tag);
        }
    }
    
    public void Apply(CommentCreatedDomainEvent @event)
    {
        var comment = new Comment(@event.Id, @event.UserId, @event.Content);
        _comments.Add(comment);
    }
    
    public void Apply(CommentUpdatedDomainEvent @event) 
    {
        var comment = _comments.First(c => c.Id == @event.Id);
        comment.Content = @event.Content;
    }
    
    public void Apply(CommentDeletedDomainEvent @event)
    {
        var comment = _comments.First(c => c.Id == @event.Id);
        _comments.Remove(comment);
    }
    
    public void Apply(TagAddedDomainEvent @event)
    {
        // Use constructor instead of Tag.Create since tag was validated before event was created
        _tags.Add(new Tag(@event.TagValue, @event.TagType));
    }
}