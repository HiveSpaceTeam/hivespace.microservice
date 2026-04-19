using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.OrderService.Domain.External;

public class SkuRef : IAuditable
{
    public long Id { get; private set; }
    public long ProductId { get; private set; }
    public string SkuNo { get; private set; } = string.Empty;
    public string SkuName { get; private set; } = string.Empty;
    public long Price { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }
    public string? Attributes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private SkuRef() { }

    public SkuRef(long id, long productId, string skuNo, long price, string currency, string? imageUrl, string? attributes, string skuName = "")
    {
        Id = id;
        ProductId = productId;
        SkuNo = skuNo;
        SkuName = skuName;
        Price = price;
        Currency = currency;
        ImageUrl = imageUrl;
        Attributes = attributes;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string skuNo, long price, string currency, string? imageUrl, string? attributes, string skuName = "")
    {
        SkuNo = skuNo;
        SkuName = skuName;
        Price = price;
        Currency = currency;
        ImageUrl = imageUrl;
        Attributes = attributes;
    }
}
