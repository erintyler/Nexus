using Nexus.Domain.Enums;
using Nexus.Domain.Primitives;

namespace Nexus.Domain.Events.Tags;

public record TagAddedDomainEvent(string TagValue, TagType TagType) : INexusEvent
{
    public string EventName { get; } = "Tag added";
    public string Description { get; } = $"{TagType}:{TagValue}";
}
