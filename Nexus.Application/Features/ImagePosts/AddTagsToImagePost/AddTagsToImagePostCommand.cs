using Nexus.Application.Common.Models;

namespace Nexus.Application.Features.ImagePosts.AddTagsToImagePost;

public record AddTagsToImagePostCommand(Guid ImagePostId, IReadOnlyList<TagDto> Tags);