using Nexus.Domain.ValueObjects;

namespace Nexus.Domain.Abstractions;

public interface ITaggable
{
    IReadOnlySet<Tag> Tags { get; }
}