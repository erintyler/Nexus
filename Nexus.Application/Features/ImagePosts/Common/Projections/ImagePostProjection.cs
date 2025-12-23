using Marten.Events.Aggregation;
using Nexus.Application.Common.Services;
using Nexus.Application.Features.ImagePosts.Common.Models;
using Nexus.Domain.Enums;
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
    public static void Apply(ImagePostReadModel readModel, ImagePostCreatedDomainEvent @event)
    {
        readModel.Title = @event.Title;
        readModel.CreatedBy = @event.UserId.ToString();
        readModel.Tags = @event.Tags
            .Select(t => new TagReadModel(t.Value, t.Type))
            .ToList();
    }
    
    public static void Apply(ImagePostReadModel readModel, CommentCreatedDomainEvent @event)
    {
        readModel.Comments.Add(new CommentReadModel(@event.Id, @event.UserId, @event.Content));
    }
    
    public static void Apply(ImagePostReadModel readModel, CommentUpdatedDomainEvent @event)
    {
        var comment = readModel.Comments.First(c => c.Id == @event.Id);
        comment.Content = @event.Content;
    }
    
    public static void Apply(ImagePostReadModel readModel, CommentDeletedDomainEvent @event)
    {
        var comment = readModel.Comments.First(c => c.Id == @event.Id);
        readModel.Comments.Remove(comment);
    }
    
    public static void Apply(ImagePostReadModel readModel, TagAddedDomainEvent @event)
    {
        // Idempotent: only add if tag doesn't already exist
        var tagExists = readModel.Tags.Any(t => 
            t.Type == @event.TagType && 
            t.Value == @event.TagValue);
        
        if (!tagExists)
        {
            readModel.Tags.Add(new TagReadModel(@event.TagValue, @event.TagType));
        }
    }
    
    public static void Apply(ImagePostReadModel readModel, TagRemovedDomainEvent @event)
    {
        // Idempotent: only remove if tag exists
        var tag = readModel.Tags.FirstOrDefault(t => 
            t.Type == @event.TagType && 
            t.Value == @event.TagValue);
        
        if (tag is not null)
        {
            readModel.Tags.Remove(tag);
        }
    }
    
    public static void Apply(ImagePostReadModel readModel, TagMigratedDomainEvent @event)
    {
        // Remove source tag if it exists
        var sourceTag = readModel.Tags.FirstOrDefault(t => 
            t.Type == @event.Source.Type && 
            t.Value == @event.Source.Value);
        
        if (sourceTag is not null)
        {
            readModel.Tags.Remove(sourceTag);
        }
        
        // Add target tag only if it doesn't already exist (idempotent)
        var targetTagExists = readModel.Tags.Any(t => 
            t.Type == @event.Target.Type && 
            t.Value == @event.Target.Value);
        
        if (!targetTagExists)
        {
            readModel.Tags.Add(new TagReadModel(@event.Target.Value, @event.Target.Type));
        }
    }
    
    public static void Apply(ImagePostReadModel readModel, StatusChangedDomainEvent @event)
    {
        readModel.Status = @event.UploadStatus;
    }
}



