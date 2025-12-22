namespace Nexus.Application.Common.Pagination;

public record PaginationRequest
{
    public int PageNumber { get; init; } = PaginationConstants.DefaultPageNumber;
    public int PageSize { get; init; } = PaginationConstants.DefaultPageSize;
    public int Skip => (PageNumber - 1) * PageSize;
}