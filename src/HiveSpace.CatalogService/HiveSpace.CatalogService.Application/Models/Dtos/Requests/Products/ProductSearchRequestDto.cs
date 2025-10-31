namespace HiveSpace.CatalogService.Application.Models.Dtos.Request.Product;

public record ProductSearchRequestDto(
    string Keyword = "",
    string Sort = "ASC",
    int PageSize = 10,
    int PageIndex = 0
);
