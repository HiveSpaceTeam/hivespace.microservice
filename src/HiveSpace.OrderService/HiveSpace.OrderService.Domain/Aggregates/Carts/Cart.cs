using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.OrderService.Domain.Exceptions;

namespace HiveSpace.OrderService.Domain.Aggregates.Carts;

public class Cart : AggregateRoot<Guid>, IAuditable
{
    public Guid UserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    private readonly List<CartItem> _items = [];
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    // EF Core constructor
    private Cart() { }

    private Cart(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Cart Create(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new InvalidFieldException(OrderDomainErrorCode.CartUserIdRequired, nameof(userId));

        return new Cart(userId);
    }

    /// <summary>
    /// Add item to cart or update quantity if already exists
    /// </summary>
    public void AddItem(Guid productId, long skuId, int quantity)
    {
        if (productId == Guid.Empty)
            throw new InvalidFieldException(OrderDomainErrorCode.CartProductIdRequired, nameof(productId));
        
        if (skuId <= 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CartSkuIdRequired, nameof(skuId));
        
        if (quantity <= 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CartInvalidQuantity, nameof(quantity));

        var existingItem = _items.FirstOrDefault(i =>
            i.ProductId == productId && i.SkuId == skuId);

        if (existingItem is not null)
        {
            // Update existing item
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            // Add new item
            var item = CartItem.Create(productId, skuId, quantity);
            _items.Add(item);
        }
    }

    /// <summary>
    /// Update quantity of specific item
    /// </summary>
    public void UpdateItemQuantity(Guid productId, long skuId, int newQuantity)
    {
        if (newQuantity <= 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CartInvalidQuantity, nameof(newQuantity));

        var item = _items.FirstOrDefault(i =>
            i.ProductId == productId && i.SkuId == skuId) ?? throw new NotFoundException(OrderDomainErrorCode.CartItemNotFound, nameof(CartItem));
        item.UpdateQuantity(newQuantity);
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    public void RemoveItem(Guid productId, long skuId)
    {
        var item = _items.FirstOrDefault(i =>
            i.ProductId == productId && i.SkuId == skuId) ?? throw new NotFoundException(OrderDomainErrorCode.CartItemNotFound, nameof(CartItem));
        _items.Remove(item);
    }

    /// <summary>
    /// Clear all items from cart (typically after order creation)
    /// </summary>
    public void Clear()
    {
        _items.Clear();
    }

    /// <summary>
    /// Get total number of items in cart
    /// </summary>
    public int GetTotalItemCount()
    {
        return _items.Sum(i => i.Quantity);
    }

    /// <summary>
    /// Check if cart is empty
    /// </summary>
    public bool IsEmpty() => _items.Count == 0;

    /// <summary>
    /// Validate cart before checkout
    /// </summary>
    public void ValidateForCheckout()
    {
        if (IsEmpty())
            throw new InvalidFieldException(OrderDomainErrorCode.CartEmpty, nameof(Cart));

        // Additional validations can be added here
        // - Check stock availability
        // - Validate products still exist
        // - Check price changes
    }
}