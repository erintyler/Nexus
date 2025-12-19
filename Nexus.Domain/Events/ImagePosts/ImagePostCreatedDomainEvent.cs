using Nexus.Domain.Primitives;
using Nexus.Domain.ValueObjects;

namespace Nexus.Domain.Events.ImagePosts;

public record ImagePostCreatedDomainEvent(Guid Id, string Title, IReadOnlyList<Tag> Tags);
