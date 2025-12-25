using Nexus.Application.Common.Pagination;

namespace Nexus.Application.Extensions;

public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        public IEnumerable<T> ApplyPagination(PaginationRequest paginationRequest)
        {
            return source
                .Skip(paginationRequest.Skip)
                .Take(paginationRequest.PageSize);
        }

        public PagedResult<T> ToPagedResult(PaginationRequest paginationRequest, int totalCount)
        {
            var validPaginationRequest = paginationRequest.WithValidPageNumber(totalCount);
            var items = source
                .ApplyPagination(validPaginationRequest)
                .ToList();

            return PagedResult<T>.Create(items, totalCount, validPaginationRequest);
        }
    }
}