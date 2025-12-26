using Nexus.Domain.Enums;

namespace Nexus.Application.Features.ImagePosts.Common.Models;

/// <summary>
/// Read model for Tag within an ImagePost projection
/// </summary>
public record TagReadModel(string Value, TagType Type);
