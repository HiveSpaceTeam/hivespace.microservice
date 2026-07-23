using System.Text.Json.Serialization;
using HiveSpace.Application.Shared.Dtos;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.OrderService.Domain.Enumerations;

namespace HiveSpace.OrderService.Application.Cart.Dtos;

public record CartItemDto
{
    public CartItemDto()
    {
    }

    public CartItemDto(
        Guid cartItemId,
        long productId,
        long skuId,
        int quantity,
        bool isSelected,
        string? productName,
        string? productThumbnailUrl,
        ProductStatus? productStatus,
        long? originalPrice,
        long? price,
        string? currency,
        string? skuNo,
        string? skuName,
        string? skuImageUrl,
        string? skuAttributes,
        Guid storeId,
        string? storeName,
        SellerStatus? storeStatus,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt)
    {
        CartItemId = cartItemId;
        ProductId = productId;
        SkuId = skuId;
        Quantity = quantity;
        IsSelected = isSelected;
        ProductName = productName;
        ProductThumbnailUrl = productThumbnailUrl;
        ProductStatus = productStatus;
        OriginalPriceAmount = originalPrice;
        PriceAmount = price;
        Currency = currency;
        SkuNo = skuNo;
        SkuName = skuName;
        SkuImageUrl = skuImageUrl;
        SkuAttributes = skuAttributes;
        StoreId = storeId;
        StoreName = storeName;
        StoreStatus = storeStatus;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid CartItemId { get; init; }
    public long ProductId { get; init; }
    public long SkuId { get; init; }
    public int Quantity { get; init; }
    public bool IsSelected { get; init; }
    public string? ProductName { get; init; }
    public string? ProductThumbnailUrl { get; init; }
    public ProductStatus? ProductStatus { get; init; }
    public MoneyResponseDto? OriginalPrice => ToMoney(OriginalPriceAmount);
    public MoneyResponseDto? Price => ToMoney(PriceAmount);
    public string? SkuNo { get; init; }
    public string? SkuName { get; init; }
    public string? SkuImageUrl { get; init; }
    public string? SkuAttributes { get; init; }
    public Guid StoreId { get; init; }
    public string? StoreName { get; init; }
    public SellerStatus? StoreStatus { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }

    [JsonIgnore]
    public long? OriginalPriceAmount { get; init; }

    [JsonIgnore]
    public long? PriceAmount { get; init; }

    [JsonIgnore]
    public string? Currency { get; init; }

    private MoneyResponseDto? ToMoney(long? amount)
    {
        if (!amount.HasValue)
            return null;

        return string.IsNullOrWhiteSpace(Currency)
            ? MoneyResponseDto.Invalid(amount.Value, issueCode: "missing_currency")
            : MoneyResponseDto.Valid(amount.Value, Currency);
    }
}
