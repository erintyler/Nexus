using Nexus.Domain.Enums;
using Nexus.Domain.Primitives;

namespace Nexus.Domain.Events.Tags;

public record TagRemovedDomainEvent(string TagValue, TagType TagType) : IDomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
}
