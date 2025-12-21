using Nexus.Domain.Enums;
using Nexus.Domain.Primitives;

namespace Nexus.Domain.Events.Tags;

public record TagRemovedDomainEvent(string TagValue, TagType TagType) : INexusEvent
{
    public string EventName { get; } = "Tag removed";
    public string Description { get; } = $"{TagType}:{TagValue}";
}
