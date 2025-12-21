using Marten.Events.Aggregation;
using Nexus.Domain.Entities;
using Nexus.Domain.Events.ImagePosts;

namespace Nexus.Application.Features.ImagePosts.Common.Projections;

public class ImagePostProjection : SingleStreamProjection<ImagePost, Guid>
{
    public void Apply(ImagePostCreatedDomainEvent @event, ImagePost imagePost)
    {
        imagePost.Title = @event.Title;
    }
}