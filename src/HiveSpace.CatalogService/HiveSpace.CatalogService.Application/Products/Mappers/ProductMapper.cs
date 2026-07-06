using HiveSpace.CatalogService.Application.Products.Dtos;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.Domain.Shared.Enumerations;

namespace HiveSpace.CatalogService.Application.Products.Mappers;

public static class ProductMapper
{
    public static ProductSummaryDto ToSummaryDto(this Product product)
    {
        var sku = product.Skus.FirstOrDefault();
        var price = sku is null
            ? new ProductMoneyDto(0L, null, false, "missing_currency")
            : new ProductMoneyDto(
                sku.Price.Amount,
                sku.Price.Currency.GetCode(),
                true,
                null);

        return new ProductSummaryDto(
            Id: product.Id,
            Name: product.Name,
            Price: price,
            ImageURL: product.ThumbnailUrl
        );
    }
}
