using Nexus.Domain.Abstractions;
using Nexus.Domain.Events.Tags;
using Nexus.Domain.ValueObjects;

namespace Nexus.Domain.Deciders;

public class TaggingDecider
{
    public static IEnumerable<object> DetermineNewTags(ITaggable state, IReadOnlyList<Tag> tags)
    {
        return tags
            .Except(state.Tags)
            .Select(t => new TagAddedDomainEvent(t.Value, t.Type));
    }
}