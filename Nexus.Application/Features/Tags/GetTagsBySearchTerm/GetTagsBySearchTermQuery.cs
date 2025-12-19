using Nexus.Application.Common.Pagination;

namespace Nexus.Application.Features.Tags.GetTagsBySearchTerm;

public record GetTagsBySearchTermQuery(string SearchTerm) : PaginationRequest;