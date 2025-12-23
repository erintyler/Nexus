using Nexus.Application.Common.Models;

namespace Nexus.Application.Features.ImagePosts.CreateImagePost;

public record CreateImagePostCommand(string Title, IReadOnlyList<TagDto> Tags, string ContentType);