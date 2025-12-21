using Marten.Events.Aggregation;
using Nexus.Application.Features.ImagePosts.Common.Models;
using Nexus.Domain.Events.Comments;
using Nexus.Domain.Events.ImagePosts;
using Nexus.Domain.Events.Tags;

namespace Nexus.Application.Features.ImagePosts.Common.Projections;

/// <summary>
/// Marten projection for ImagePost - builds a read model from domain events.
/// This creates a denormalized view optimized for queries.
/// </summary>
public class ImagePostProjection : SingleStreamProjection<ImagePostReadModel, Guid>
{
    public ImagePostProjection()
    {
        ProjectEvent<ImagePostCreatedDomainEvent>(Apply);
        ProjectEvent<CommentCreatedDomainEvent>(Apply);
        ProjectEvent<CommentUpdatedDomainEvent>(Apply);
        ProjectEvent<CommentDeletedDomainEvent>(Apply);
        ProjectEvent<TagAddedDomainEvent>(Apply);
    }
    
    private static void Apply(ImagePostReadModel readModel, ImagePostCreatedDomainEvent @event)
    {
        readModel.Title = @event.Title;
        readModel.CreatedBy = @event.UserId.ToString();
        readModel.Tags = @event.Tags
            .Select(t => new TagReadModel(t.Value, t.Type))
            .ToList();
    }
    
    private static void Apply(ImagePostReadModel readModel, CommentCreatedDomainEvent @event)
    {
        readModel.Comments.Add(new CommentReadModel(@event.Id, @event.UserId, @event.Content));
    }
    
    private static void Apply(ImagePostReadModel readModel, CommentUpdatedDomainEvent @event)
    {
        var comment = readModel.Comments.First(c => c.Id == @event.Id);
        comment.Content = @event.Content;
    }
    
    private static void Apply(ImagePostReadModel readModel, CommentDeletedDomainEvent @event)
    {
        var comment = readModel.Comments.First(c => c.Id == @event.Id);
        readModel.Comments.Remove(comment);
    }
    
    private static void Apply(ImagePostReadModel readModel, TagAddedDomainEvent @event)
    {
        readModel.Tags.Add(new TagReadModel(@event.TagValue, @event.TagType));
    }
}

