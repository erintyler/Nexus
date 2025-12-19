namespace Nexus.Application.Common.Pagination;

public class PagedResult<T>
{
    private PagedResult(IReadOnlyList<T> data, int totalCount, PaginationRequest paginationRequest)
    {
        Items = data;
        TotalCount = totalCount;
        PageNumber = paginationRequest.PageNumber;
        PageSize = paginationRequest.PageSize;
    }
    
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    
    public static PagedResult<T> Create(IReadOnlyList<T> data, int totalCount, PaginationRequest paginationRequest)
    {
        if (paginationRequest.PageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(paginationRequest), "Page number must be greater than 0.");
        }
        
        if (paginationRequest.PageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(paginationRequest), "Page size must be greater than 0.");
        }
        
        return new PagedResult<T>(data, totalCount, paginationRequest);
    }
}