using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

namespace HiveSpace.CatalogService.Application.Products.Dtos;

public record ProductDetailDto
{
    public int Id { get; init; }
    public Guid SellerId { get; init; }
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
    public List<ProductCategory> Categories { get; init; } = [];
    public List<ProductImage> Images { get; init; } = [];
    public List<ProductAttributeDto> Attributes { get; init; } = [];
    public List<Sku> Skus { get; init; } = [];
    public List<ProductVariant> Variants { get; init; } = [];
    public CurrentSellerDto? CurrentSeller { get; init; }
}

public record CurrentSellerDto(Guid Id, string StoreName, string LogoUrl);
