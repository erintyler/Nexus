using Nexus.Domain.Abstractions;
using Nexus.Domain.Common;
using Nexus.Domain.Enums;
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
public sealed class ImagePost : BaseEntity, ITaggable
{
    public const int MinTitleLength = 1;
    public const int MaxTitleLength = 200;
    
    private readonly List<Comment> _comments = [];
    private readonly HashSet<Tag> _tags = [];
    
    internal ImagePost() { }
    
    // Properties with private setters for encapsulation
    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string CreatedBy { get; private set; } = null!;
    public UploadStatus Status { get; private set; }
    
    // Marten event sourcing metadata
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastModified { get; private set; }
    public string LastModifiedBy { get; private set; } = null!;
    
    // Expose collections as read-only
    public IReadOnlyList<Comment> Comments => _comments.AsReadOnly();
    public IReadOnlySet<Tag> Tags => _tags.AsReadOnly();
    
    /// <summary>
    /// Factory method to create a new ImagePost with validation and business rules.
    /// Returns the creation event if successful.
    /// </summary>
    public static Result<ImagePostCreatedDomainEvent> Create(Guid userId, string title, IReadOnlyList<TagData> tags)
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
        
        switch (title.Length)
        {
            case < MinTitleLength:
                return ImagePostErrors.TitleTooShort;
            case > MaxTitleLength:
                return ImagePostErrors.TitleTooLong;
        }

        if (tags.Count == 0)
        {
            return ImagePostErrors.AtLeastOneTagRequired;
        }

        // Validate tags
        var tagResults = tags
            .Select(t => Tag.Create(t.Type, t.Value))
            .ToList();

        var errors = tagResults
            .WithIndexedErrors("tags")
            .ToList();
        
        if (errors.Count != 0)
        {
            return Result.Failure<ImagePostCreatedDomainEvent>(errors);
        }
        
        // Tags are validated, pass the original TagData primitives to the event
        return new ImagePostCreatedDomainEvent(userId, title, tags);
    }
    
    /// <summary>
    /// Add tags to the image post, ensuring no duplicates.
    /// Returns events for only the new tags.
    /// </summary>
    public Result<IEnumerable<TagAddedDomainEvent>> AddTags(IReadOnlyList<TagData> newTags)
    {
        // Validate tags
        var tagResults = newTags
            .Select(t => Tag.Create(t.Type, t.Value))
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
            .Select(t => new TagAddedDomainEvent(t.Type, t.Value))
            .ToList();
        
        return events.Count == 0 
            ? Result.Failure<IEnumerable<TagAddedDomainEvent>>(TagErrors.NoNewTags) 
            : Result.Success(events.AsEnumerable());
    }

    /// <summary>
    /// Remove tags from the image post.
    /// Returns events for only the tags that were present and removed.
    /// </summary>
    public Result<IEnumerable<TagRemovedDomainEvent>> RemoveTags(IReadOnlyList<TagData> tags)
    {
        // Validate tags
        var tagResults = tags
            .Select(t => Tag.Create(t.Type, t.Value))
            .ToList();

        var errors = tagResults
            .WithIndexedErrors("tags")
            .ToList();
        
        if (errors.Count != 0)
        {
            return Result.Failure<IEnumerable<TagRemovedDomainEvent>>(errors);
        }
        
        var validatedTags = tagResults.Select(tr => tr.Value).ToList();
        
        // Only create events for tags that currently exist
        var events = validatedTags
            .Intersect(_tags)
            .Select(t => new TagRemovedDomainEvent(t.Type, t.Value))
            .ToList();
        
        return events.Count == 0 
            ? Result.Failure<IEnumerable<TagRemovedDomainEvent>>(TagErrors.NoTagsToRemove)
            : Result.Success(events.AsEnumerable());
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

    /// <summary>
    /// Mark the image post as processing, validating user and current status.
    /// </summary>
    public Result<StatusChangedDomainEvent> MarkAsProcessing(Guid userId)
    {
        if (CreatedBy != userId.ToString())
        {
            //return ImagePostErrors.NotCreator;
        }
        
        if (Status is not UploadStatus.Pending)
        {
            return ImagePostErrors.InvalidStatusTransition;
        }
        
        return new StatusChangedDomainEvent(Id, UploadStatus.Processing, userId);
    }
    
    public Result<StatusChangedDomainEvent> MarkAsCompleted()
    {
        if (Status is not UploadStatus.Processing)
        {
            return ImagePostErrors.InvalidStatusTransition;
        }
        
        return new StatusChangedDomainEvent(Id, UploadStatus.Completed);
    }
    
    public Result<StatusChangedDomainEvent> MarkAsFailed()
    {
        if (Status is not UploadStatus.Processing)
        {
            return ImagePostErrors.InvalidStatusTransition;
        }
        
        return new StatusChangedDomainEvent(Id, UploadStatus.Failed);
    }
    
    // Event application methods for event sourcing
    // Note: No validation in Apply methods - events represent historical facts that were already validated
    // when created. During reconstruction, we trust the event stream.
    public void Apply(ImagePostCreatedDomainEvent @event)
    {
        Title = @event.Title;
        CreatedBy = @event.UserId.ToString();
        Status = @event.Status;
        
        // Reconstruct Tag value objects from primitive TagData stored in the event
        foreach (var tagData in @event.Tags)
        {
            _tags.Add(new Tag(tagData.Type, tagData.Value));
        }
    }
    
    public void Apply(TagAddedDomainEvent @event)
    {
        var tag = new Tag(@event.TagType, @event.TagValue);
        _tags.Add(tag);
    }
    
    public void Apply(TagRemovedDomainEvent @event)
    {
        // Idempotent: only remove if present
        var tag = _tags.FirstOrDefault(t => 
            t.Type == @event.TagType && 
            t.Value == @event.TagValue);
        
        if (tag is not null)
        {
            _tags.Remove(tag);
        }
    }
    
    public void Apply(TagMigratedDomainEvent @event)
    {
        // Remove source tag if present
        var sourceTag = _tags.FirstOrDefault(t => 
            t.Type == @event.Source.Type && 
            t.Value == @event.Source.Value);
        
        if (sourceTag is not null)
        {
            _tags.Remove(sourceTag);
        }
        
        var targetTag = new Tag(@event.Target.Type, @event.Target.Value);
        _tags.Add(targetTag);
    }
    
    public void Apply(CommentCreatedDomainEvent @event)
    {
        var comment = new Comment(@event.Id, @event.UserId, @event.Content);
        _comments.Add(comment);
    }
    
    public void Apply(CommentUpdatedDomainEvent @event) 
    {
        var comment = _comments.FirstOrDefault(c => c.Id == @event.Id);

        comment?.Content = @event.Content;
    }
    
    public void Apply(CommentDeletedDomainEvent @event)
    {
        var comment = _comments.FirstOrDefault(c => c.Id == @event.Id);
        
        if (comment is null)
        {
            return;
        }
        
        _comments.Remove(comment);
    }
    
    public void Apply(StatusChangedDomainEvent @event)
    {
        Status = @event.UploadStatus;
    }
}