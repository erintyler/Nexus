using Nexus.Domain.ValueObjects;

namespace Nexus.Domain.Events.ImagePosts;

public record ImagePostCreatedDomainEvent(string Title, IReadOnlyList<Tag> Tags) : INexusEvent
{
    public string EventName { get; } = "Image created";
    public string Description { get; } = $"Title: {Title} | Tags: {string.Join(", ", Tags.Select(t => $"{t.Type}:{t.Value}"))}";
}
