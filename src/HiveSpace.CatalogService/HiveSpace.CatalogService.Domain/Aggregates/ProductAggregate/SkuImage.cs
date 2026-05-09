using HiveSpace.CatalogService.Domain.Exceptions;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;

namespace HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;

public class SkuImage(string fileId, string? imageUrl = null) : ValueObject
{
    public string FileId { get; private set; } = fileId;
    public string? ImageUrl { get; private set; } = imageUrl;

    public SkuImage WithImageUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new InvalidFieldException(CatalogDomainErrorCode.InvalidImageUrl, nameof(url));
        return new(FileId, url);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FileId;
    }
}
