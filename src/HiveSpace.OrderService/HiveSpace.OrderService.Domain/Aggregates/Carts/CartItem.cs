using HiveSpace.Domain.Shared.IdGeneration;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.OrderService.Domain.Exceptions;

namespace HiveSpace.OrderService.Domain.Aggregates.Carts;

public class CartItem : Entity<Guid>, IAuditable
{
    public long ProductId { get; private set; }
    public long SkuId { get; private set; }
    public int Quantity { get; private set; }
    public bool IsSelected { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    // EF Core constructor
    private CartItem() { }

    private CartItem(Guid id, long productId, long skuId, int quantity)
    {
        Id = id;
        ProductId = productId;
        SkuId = skuId;
        Quantity = quantity;
        IsSelected = true;
    }

    public static CartItem Create(long productId, long skuId, int quantity)
    {
        if (productId <= 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CartProductIdRequired, nameof(productId));

        if (skuId <= 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CartSkuIdRequired, nameof(skuId));

        if (quantity <= 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CartInvalidQuantity, nameof(quantity));

        return new CartItem(IdGenerator.NewId<Guid>(), productId, skuId, quantity);
    }

    public void UpdateSku(long newSkuId)
    {
        if (newSkuId <= 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CartSkuIdRequired, nameof(newSkuId));

        SkuId = newSkuId;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CartInvalidQuantity, nameof(newQuantity));

        Quantity = newQuantity;
    }

    public void UpdateSelection(bool isSelected)
    {
        IsSelected = isSelected;
    }
}

