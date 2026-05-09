using HiveSpace.Domain.Shared.Entities;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

public class SkuImage(string fileId, string? imageUrl = null) : ValueObject
{
    public string FileId { get; private set; } = fileId;
    public string? ImageUrl { get; private set; } = imageUrl;

    public SkuImage WithImageUrl(string url) => new(FileId, url);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FileId;
    }
}
