using JasperFx.Events;
using Marten.Events.Aggregation;
using Nexus.Domain.Entities;
using Nexus.Domain.Events.Comments;
using Nexus.Domain.Events.ImagePosts;
using Nexus.Domain.Events.Tags;

namespace Nexus.Application.Features.ImagePosts.Common.Projections;

public class ImagePostProjection : SingleStreamProjection<ImagePost, Guid>
{
    public ImagePostProjection()
    {
        ProjectEvent<ImagePostCreatedDomainEvent>((post, @event) => post.Apply(@event));
        ProjectEvent<CommentCreatedDomainEvent>((post, @event) => post.Apply(@event));
        ProjectEvent<CommentUpdatedDomainEvent>((post, @event) => post.Apply(@event));
        ProjectEvent<CommentDeletedDomainEvent>((post, @event) => post.Apply(@event));
        ProjectEvent<TagAddedDomainEvent>((post, @event) => post.Apply(@event));
    }
}