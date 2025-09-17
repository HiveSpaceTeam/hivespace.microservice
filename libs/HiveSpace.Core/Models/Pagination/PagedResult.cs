namespace HiveSpace.Core.Models.Pagination;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public PaginationMetadata Pagination { get; init; } = new();

    public PagedResult(IReadOnlyList<T> items, int currentPage, int pageSize, int totalItems)
    {
        Items = items;
        Pagination = new PaginationMetadata(currentPage, pageSize, totalItems);
    }
}

public class PaginationMetadata
{
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }

    public PaginationMetadata(int currentPage, int pageSize, int totalItems)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(currentPage);
        ArgumentOutOfRangeException.ThrowIfNegative(totalItems);

        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        HasNextPage = currentPage < TotalPages;
        HasPreviousPage = currentPage > 1;
    }

    public PaginationMetadata()
    {
        // Default parameterless constructor for initialization
    }
}