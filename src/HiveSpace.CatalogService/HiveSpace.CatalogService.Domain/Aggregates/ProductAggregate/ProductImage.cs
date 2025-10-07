
using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
public class ProductImage(Guid productId, string fileId) : ValueObject
{
    public Guid ProductId { get; private set; } = productId;
    public string FileId { get; private set; } = fileId;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ProductId;
        yield return FileId;
    }
}
