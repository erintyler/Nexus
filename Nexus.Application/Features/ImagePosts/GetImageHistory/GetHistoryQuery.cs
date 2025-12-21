using Nexus.Application.Common.Pagination;

namespace Nexus.Application.Features.ImagePosts.GetImageHistory;

public record GetHistoryQuery(Guid Id, DateTimeOffset? DateFrom = null, DateTimeOffset? DateTo = null) : PaginationRequest;