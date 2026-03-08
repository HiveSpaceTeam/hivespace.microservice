using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.OrderService.Domain.Exceptions;
using System.Text.Json.Serialization;

namespace HiveSpace.OrderService.Domain.Aggregates.Carts;

public class CartItem : Entity<Guid>, IAuditable
{
    [JsonInclude]
    public Guid ProductId { get; private set; }
    [JsonInclude]
    public long SkuId { get; private set; }
    [JsonInclude]
    public int Quantity { get; private set; }
    [JsonInclude]
    public DateTimeOffset CreatedAt { get; private set; }
    [JsonInclude]
    public DateTimeOffset? UpdatedAt { get; private set; }

    // EF Core constructor
    private CartItem() { }

    [JsonConstructor]
    private CartItem(Guid productId, long skuId, int quantity)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        SkuId = skuId;
        Quantity = quantity;
    }

    public static CartItem Create(Guid productId, long skuId, int quantity)
    {
        if (productId == Guid.Empty)
            throw new InvalidFieldException(OrderDomainErrorCode.CartProductIdRequired, nameof(productId));
        
        if (skuId <= 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CartSkuIdRequired, nameof(skuId));
        
        if (quantity <= 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CartInvalidQuantity, nameof(quantity));

        return new CartItem(productId, skuId, quantity);
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CartInvalidQuantity, nameof(newQuantity));

        Quantity = newQuantity;
    }
}