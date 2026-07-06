namespace HiveSpace.CatalogService.Application.Products.Dtos;

public record ProductSkuDto(
    int Id,
    string SkuNo,
    string SkuName,
    ProductMoneyDto Price,
    int Quantity,
    bool IsActive,
    List<ProductImageDto> Images,
    string? Attributes);
