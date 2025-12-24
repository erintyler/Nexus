using Nexus.Application.Common.Models;

namespace Nexus.Application.Features.ImagePosts.RemoveTagsToImagePost;

public record RemoveTagsFromImagePostCommand(Guid Id, IReadOnlyList<TagDto> Tags);