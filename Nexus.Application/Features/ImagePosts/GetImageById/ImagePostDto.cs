using Nexus.Application.Common.Models;

namespace Nexus.Application.Features.ImagePosts.GetImageById;

public record ImagePostDto(
    string Title,
    IReadOnlyList<TagDto> Tags,
    string Url,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastUpdated,
    string CreatedBy,
    string LastUpdatedBy);