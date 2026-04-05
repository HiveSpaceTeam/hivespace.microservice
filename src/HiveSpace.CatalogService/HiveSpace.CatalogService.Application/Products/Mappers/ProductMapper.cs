using HiveSpace.CatalogService.Application.Products.Dtos;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

namespace HiveSpace.CatalogService.Application.Products.Mappers;

public static class ProductMapper
{
    public static ProductSummaryDto ToSummaryDto(this Product product)
    {
        var sku = product.Skus.FirstOrDefault();
        var image = sku?.Images.FirstOrDefault();

        return new ProductSummaryDto(
            Id: product.Id,
            Name: product.Name,
            Price: sku?.Price.Amount ?? 0m,
            ImageURL: image?.FileId ?? string.Empty
        );
    }
}
