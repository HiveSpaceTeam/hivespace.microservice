using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
public class ProductSku(string productId, string SkuId) : ValueObject
{
    public string ProductId { get; private set; } = productId;
    public string SkuId { get; private set; } = SkuId;
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ProductId;
        yield return SkuId;
    }
}