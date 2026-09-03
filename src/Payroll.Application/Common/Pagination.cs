namespace Payroll.Application.Common;

// Pagination:
public record PaginationQuery(int PageNumber = 1, int PageSize = 10)
{
    public int ValidPageNumber => Math.Max(1, PageNumber);
    public int ValidPageSize => Math.Clamp(PageSize, 1, 100);
}

// Pagination:
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}