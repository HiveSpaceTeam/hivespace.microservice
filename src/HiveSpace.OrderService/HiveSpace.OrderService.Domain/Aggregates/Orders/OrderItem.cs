using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Domain.ValueObjects;

namespace HiveSpace.OrderService.Domain.Aggregates.Orders;

public class OrderItem : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public Guid SkuId { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = null!;
    public Money LineTotal { get; private set; } = null!;
    public bool IsCOD { get; private set; }
    public ProductSnapshot ProductSnapshot { get; private set; } = null!;
    
    private OrderItem() { }

    public static OrderItem Create(
        Guid productId,
        Guid skuId,
        int quantity,
        Money unitPrice,
        ProductSnapshot productSnapshot,
        bool isCOD = false)
    {
        if (quantity <= 0)
            throw new DomainException(400, OrderDomainErrorCode.InvalidQuantity, nameof(quantity));
        if (unitPrice == null || unitPrice.Amount <= 0)
            throw new DomainException(400, OrderDomainErrorCode.InvalidPrice, nameof(unitPrice));

        var item = new OrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            SkuId = skuId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            ProductSnapshot = productSnapshot,
            IsCOD = isCOD
        };
        
        item.CalculateLineTotal();
        return item;
    }

    private void CalculateLineTotal()
    {
        LineTotal = UnitPrice * Quantity;
    }
}
