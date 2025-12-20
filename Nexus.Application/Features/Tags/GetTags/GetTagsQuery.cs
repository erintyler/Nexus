using Nexus.Application.Common.Pagination;

namespace Nexus.Application.Features.Tags.GetTags;

public record GetTagsQuery(string? SearchTerm) : PaginationRequest;