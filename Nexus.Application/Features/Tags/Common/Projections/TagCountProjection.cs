using Marten.Events.Projections;
using Nexus.Domain.Events.Tags;

namespace Nexus.Application.Features.Tags.Common.Projections;

public class TagCountProjection : MultiStreamProjection<TagCount, string>
{
    public TagCountProjection()
    {
        Identity<TagAddedDomainEvent>(x => TagCount.GetId(x.TagValue, x.TagType));
        Identity<TagRemovedDomainEvent>(x => TagCount.GetId(x.TagValue, x.TagType));
        
        DeleteEvent<TagRemovedDomainEvent>((tagCount, _) => tagCount.Count == 0);
    }
    
    public TagCount Create(TagAddedDomainEvent @event)
    {
        return new TagCount
        {
            Id = TagCount.GetId(@event.TagValue, @event.TagType),
            TagValue = @event.TagValue,
            TagType = @event.TagType,
            Count = 1
        };
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