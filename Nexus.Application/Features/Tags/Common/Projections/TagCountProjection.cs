using JasperFx.Events;
using Marten.Events.Projections;
using Nexus.Domain.Events.ImagePosts;
using Nexus.Domain.Events.Tags;

namespace Nexus.Application.Features.Tags.Common.Projections;

public class TagCountProjection : MultiStreamProjection<TagCount, string>
{
    public TagCountProjection()
    {
        Identities<ImagePostCreatedDomainEvent>(x => x.Tags.
            Select(tag => TagCount.GetId(tag.Type, tag.Value))
            .ToList());

        Identity<TagAddedDomainEvent>(x => TagCount.GetId(x.TagType, x.TagValue));
        Identity<TagRemovedDomainEvent>(x => TagCount.GetId(x.TagType, x.TagValue));

        DeleteEvent<TagRemovedDomainEvent>((tagCount, _) => tagCount.Count == 0);
    }

    public void Apply(ImagePostCreatedDomainEvent @event, TagCount current)
    {
        current.Count += 1;
    }

    public void Apply(TagAddedDomainEvent @event, TagCount current)
    {
        current.Count += 1;
    }

    public void Apply(TagRemovedDomainEvent @event, TagCount current)
    {
        current.Count = Math.Max(0, current.Count - 1);
    }
}