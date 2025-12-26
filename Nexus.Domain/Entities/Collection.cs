using Nexus.Domain.Abstractions;
using Nexus.Domain.Common;
using Nexus.Domain.Errors;
using Nexus.Domain.Events.Collections;
using Nexus.Domain.Primitives;
using Nexus.Domain.ValueObjects;

namespace Nexus.Domain.Entities;

/// <summary>
/// Collection aggregate root - represents a collection of image posts.
/// This entity follows event sourcing and is reconstructed from domain events.
/// Encapsulates business rules and generates domain events.
/// Collections group image posts and aggregate their tags.
/// </summary>
public sealed class Collection : BaseEntity, ITaggable
{
    public const int MinTitleLength = 5;
    public const int MaxTitleLength = 200;

    private readonly HashSet<Guid> _imagePostIds = [];
    private readonly HashSet<Tag> _aggregatedTags = [];

    internal Collection() { }
    internal Collection(Guid id) : base(id) { }

    // Properties with private setters for encapsulation
    public string Title { get; private set; } = null!;
    public string CreatedBy { get; private set; } = null!;

    // Marten event sourcing metadata
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastModified { get; private set; }
    public string LastModifiedBy { get; private set; } = null!;

    // Expose collections as read-only
    public IReadOnlySet<Guid> ImagePostIds => _imagePostIds.AsReadOnly();
    public IReadOnlySet<Tag> Tags => _aggregatedTags.AsReadOnly();

    /// <summary>
    /// Factory method to create a new Collection with validation and business rules.
    /// Returns the creation event if successful.
    /// </summary>
    public static Result<CollectionCreatedDomainEvent> Create(Guid userId, string title)
    {
        // Validate userId
        if (userId == Guid.Empty)
        {
            return CollectionErrors.UserIdEmpty;
        }

        // Validate title
        if (string.IsNullOrWhiteSpace(title))
        {
            return CollectionErrors.TitleEmpty;
        }

        switch (title.Length)
        {
            case < MinTitleLength:
                return CollectionErrors.TitleTooShort;
            case > MaxTitleLength:
                return CollectionErrors.TitleTooLong;
        }

        return new CollectionCreatedDomainEvent(userId, title);
    }

    /// <summary>
    /// Add an image post to the collection.
    /// Returns event if the image post is not already in the collection.
    /// </summary>
    public Result<ImagePostAddedToCollectionDomainEvent> AddImagePost(Guid imagePostId)
    {
        if (imagePostId == Guid.Empty)
        {
            return ImagePostErrors.NotFound;
        }

        if (_imagePostIds.Contains(imagePostId))
        {
            return CollectionErrors.ImagePostAlreadyExists;
        }

        return new ImagePostAddedToCollectionDomainEvent(imagePostId);
    }

    /// <summary>
    /// Remove an image post from the collection.
    /// Returns event if the image post is currently in the collection.
    /// </summary>
    public Result<ImagePostRemovedFromCollectionDomainEvent> RemoveImagePost(Guid imagePostId)
    {
        if (!_imagePostIds.Contains(imagePostId))
        {
            return CollectionErrors.ImagePostNotFound;
        }

        return new ImagePostRemovedFromCollectionDomainEvent(imagePostId);
    }

    // Event application methods for event sourcing
    // Note: No validation in Apply methods - events represent historical facts that were already validated
    // when created. During reconstruction, we trust the event stream.
    public void Apply(CollectionCreatedDomainEvent @event)
    {
        Title = @event.Title;
        CreatedBy = @event.UserId.ToString();
    }

    public void Apply(ImagePostAddedToCollectionDomainEvent @event)
    {
        _imagePostIds.Add(@event.ImagePostId);
    }

    public void Apply(ImagePostRemovedFromCollectionDomainEvent @event)
    {
        _imagePostIds.Remove(@event.ImagePostId);
    }
}
