using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Enums;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.ValueObjects;

namespace HiveSpace.OrderService.Domain.Entities;

public class OrderItem : Entity<Guid>
{
    public int SkuId { get; private set; }

    public string ProductName { get; private set; }

    public string VariantName { get; private set; }

    public string Thumbnail { get; private set; }

    public int Quantity { get; private set; }

    public Money Price { get; private set; }

    private OrderItem()
    {
        ProductName = string.Empty;
        VariantName = string.Empty;
        Thumbnail = string.Empty;
        Price = null!;
    }

    public OrderItem(int skuId, string productName, string variantName, string thumbnail, int quantity, double amount, Currency currency)
    {
        SkuId = skuId;
        ProductName = productName;
        VariantName = variantName;
        Thumbnail = thumbnail;
        Quantity = quantity;
        Price = new Money(amount, currency);

        if (IsInvalid())
        {
            throw new DomainException(400, OrderErrorCode.InvalidOrder, nameof(OrderItem));
        }
    }

    private bool IsInvalid()
    {
        return Quantity <= 0 || string.IsNullOrEmpty(ProductName) || string.IsNullOrEmpty(VariantName) || string.IsNullOrEmpty(Thumbnail);
    }
}