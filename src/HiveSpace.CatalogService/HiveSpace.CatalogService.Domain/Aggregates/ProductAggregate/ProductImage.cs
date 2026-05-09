using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

public class ProductImage(int productId, string fileId, string? imageUrl = null) : ValueObject
{
    public int ProductId { get; private set; } = productId;
    public string FileId { get; private set; } = fileId;
    public string? ImageUrl { get; private set; } = imageUrl;

    public ProductImage WithImageUrl(string url) => new(ProductId, FileId, url);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ProductId;
        yield return FileId;
    }
}
