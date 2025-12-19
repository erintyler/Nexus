using Nexus.Application.Common.Pagination;

namespace Nexus.Application.Extensions;

public static class PaginationExtensions
{
    extension(PaginationRequest paginationRequest)
    {
        public PaginationRequest WithValidPageNumber(int totalCount)
        {
            var totalPages = (int)Math.Ceiling(totalCount / (double)paginationRequest.PageSize);
            var validPageNumber = Math.Clamp(paginationRequest.PageNumber, 1, Math.Max(totalPages, 1));

            return paginationRequest with { PageNumber = validPageNumber };
        }
    }
}