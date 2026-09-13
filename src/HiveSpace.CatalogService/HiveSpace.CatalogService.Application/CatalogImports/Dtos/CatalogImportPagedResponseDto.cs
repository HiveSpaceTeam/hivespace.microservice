namespace HiveSpace.CatalogService.Application.CatalogImports.Dtos;

public record CatalogImportPagedResponseDto<T>(
    IReadOnlyList<T> Data,
    CatalogImportPaginationDto Pagination);

public record CatalogImportPaginationDto(
    int CurrentPage,
    int PageSize,
    int TotalItems,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage)
{
    public static CatalogImportPaginationDto Create(int currentPage, int pageSize, int totalItems)
    {
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        return new CatalogImportPaginationDto(
            currentPage,
            pageSize,
            totalItems,
            totalPages,
            currentPage > 1,
            currentPage < totalPages);
    }
}
