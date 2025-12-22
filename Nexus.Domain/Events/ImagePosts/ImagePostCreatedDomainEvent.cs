using Nexus.Domain.Primitives;

namespace Nexus.Domain.Events.ImagePosts;

public record ImagePostCreatedDomainEvent(Guid UserId, string Title, IReadOnlyList<TagData> Tags) : INexusEvent
{
    public string EventName { get; } = "Image created";
    public string Description { get; } = $"User: {UserId} | Title: {Title} | Tags: {string.Join(", ", Tags.Select(t => $"{t.Type}:{t.Value}"))}";
}
