using Nexus.Domain.Enums;
using Nexus.Domain.Primitives;

namespace Nexus.Domain.Events.Tags;

public record TagRemovedDomainEvent(TagType TagType, string TagValue) : INexusEvent
{
    public string EventName { get; } = "Tag removed";
    public string Description { get; } = $"{TagType}:{TagValue}";
}
