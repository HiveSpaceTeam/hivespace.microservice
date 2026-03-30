using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.OrderService.Domain.External;

public class ProductRef : IAuditable
{
    public long Id { get; private set; }
    public Guid StoreId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? ThumbnailUrl { get; private set; }
    public ProductStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private ProductRef() { }

    public ProductRef(long id, Guid storeId, string name, string? thumbnailUrl, ProductStatus status)
    {
        Id = id;
        StoreId = storeId;
        Name = name;
        ThumbnailUrl = thumbnailUrl;
        Status = status;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(Guid storeId, string name, string? thumbnailUrl, ProductStatus status)
    {
        StoreId = storeId;
        Name = name;
        ThumbnailUrl = thumbnailUrl;
        Status = status;
    }
}
