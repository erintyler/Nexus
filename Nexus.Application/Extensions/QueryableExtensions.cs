using Marten;
using Nexus.Application.Common.Pagination;

namespace Nexus.Application.Extensions;

public static class QueryableExtensions
{
    extension<T>(IQueryable<T> query) where T : notnull
    {
        public IQueryable<T> ApplyPagination(PaginationRequest paginationRequest)
        {
            return query
                .Skip(paginationRequest.Skip)
                .Take(paginationRequest.PageSize);
        }

        public async Task<PagedResult<T>> ToPagedResultAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken = default)
        {
            var totalCount = await query.CountAsync(cancellationToken);
            
            var validPaginationRequest = paginationRequest.WithValidPageNumber(totalCount);
            var items = await query
                .ApplyPagination(validPaginationRequest)
                .ToListAsync(cancellationToken);

            return PagedResult<T>.Create(items, totalCount, validPaginationRequest);
        }
    }
}