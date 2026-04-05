namespace HiveSpace.CatalogService.Application.Contracts;

public record ProductSearchRequestDto(
    string Keyword = "",
    string Sort = "ASC",
    int PageSize = 10,
    int PageIndex = 0
);
