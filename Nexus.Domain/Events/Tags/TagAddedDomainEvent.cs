using Nexus.Domain.Enums;
using Nexus.Domain.Primitives;

namespace Nexus.Domain.Events.Tags;

public record TagAddedDomainEvent(TagType TagType, string TagValue) : INexusEvent
{
    public string EventName { get; } = "Tag added";
    public string Description { get; } = $"{TagType}:{TagValue}";
}
