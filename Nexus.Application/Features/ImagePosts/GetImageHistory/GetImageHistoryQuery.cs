using Nexus.Application.Common.Pagination;

namespace Nexus.Application.Features.ImagePosts.GetImageHistory;

public record GetImageHistoryQuery(Guid Id, DateTimeOffset? DateFrom = null, DateTimeOffset? DateTo = null) : PaginationRequest;