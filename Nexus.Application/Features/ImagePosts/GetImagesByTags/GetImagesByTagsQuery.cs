using Nexus.Application.Common.Models;
using Nexus.Application.Common.Pagination;

namespace Nexus.Application.Features.ImagePosts.GetImagesByTags;

public record GetImagesByTagsQuery(IReadOnlyList<TagDto> Tags) : PaginationRequest;