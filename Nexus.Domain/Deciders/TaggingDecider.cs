using Nexus.Domain.Abstractions;
using Nexus.Domain.Common;
using Nexus.Domain.Events.Tags;
using Nexus.Domain.Extensions;
using Nexus.Domain.ValueObjects;

namespace Nexus.Domain.Deciders;

public static class TaggingDecider
{
    public static Result<IEnumerable<TagAddedDomainEvent>> DetermineNewTags(ITaggable state, IReadOnlyList<Tag> tags)
    {
        var errors = tags
            .Select(t => Tag.Create(t.Type, t.Value))
            .WithIndexedErrors(nameof(tags))
            .ToList();

        if (errors.Count != 0)
        {
            return Result.Failure<IEnumerable<TagAddedDomainEvent>>(errors);
        }
        
        var events = tags
            .Except(state.Tags)
            .Select(t => new TagAddedDomainEvent(t.Value, t.Type));
        
        return Result.Success(events);
    }
}