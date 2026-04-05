using HiveSpace.Domain.Shared.IdGeneration;
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

    private Cart(Guid id, Guid userId)
    {
        Id = id;
        UserId = userId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Cart Create(Guid userId, Guid? id = null)
    {
        if (userId == Guid.Empty)
            throw new InvalidFieldException(OrderDomainErrorCode.CartUserIdRequired, nameof(userId));

        var cartId = (id.HasValue && id.Value != Guid.Empty) ? id.Value : IdGenerator.NewId<Guid>();
        return new Cart(cartId, userId);
    }

    /// <summary>
    /// Add item to cart or update quantity if already exists
    /// </summary>
    public void AddItem(long productId, long skuId, int quantity)
    {
        if (productId <= 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CartProductIdRequired, nameof(productId));

        if (skuId <= 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CartSkuIdRequired, nameof(skuId));

        if (quantity <= 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CartInvalidQuantity, nameof(quantity));

        var existingItem = _items.FirstOrDefault(i =>
            i.ProductId == productId && i.SkuId == skuId);

        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var item = CartItem.Create(productId, skuId, quantity);
            _items.Add(item);
        }
    }

    /// <summary>
    /// Update quantity of specific item
    /// </summary>
    public void UpdateItemQuantity(long productId, long skuId, int newQuantity)
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
    public void RemoveItem(long productId, long skuId)
    {
        var item = _items.FirstOrDefault(i =>
            i.ProductId == productId && i.SkuId == skuId) ?? throw new NotFoundException(OrderDomainErrorCode.CartItemNotFound, nameof(CartItem));
        _items.Remove(item);
    }

    /// <summary>
    /// Remove item by cart item ID
    /// </summary>
    public void RemoveItemById(Guid cartItemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == cartItemId)
            ?? throw new NotFoundException(OrderDomainErrorCode.CartItemNotFound, nameof(CartItem));
        _items.Remove(item);
    }

    /// <summary>
    /// Update quantity and/or selected state of a specific item
    /// </summary>
    public void UpdateItemById(Guid cartItemId, long? skuId, int? quantity, bool? isSelected)
    {
        var item = _items.FirstOrDefault(i => i.Id == cartItemId)
            ?? throw new NotFoundException(OrderDomainErrorCode.CartItemNotFound, nameof(CartItem));

        if (skuId.HasValue)
            item.UpdateSku(skuId.Value);

        if (quantity.HasValue)
            item.UpdateQuantity(quantity.Value);

        if (isSelected.HasValue)
            item.UpdateSelection(isSelected.Value);
    }

    /// <summary>
    /// Select or deselect all items in cart
    /// </summary>
    public void SelectAllItems(bool isSelected)
    {
        foreach (var item in _items)
            item.UpdateSelection(isSelected);
    }

    /// <summary>
    /// Clear all items from cart (typically after order creation)
    /// </summary>
    public void Clear()
    {
        _items.Clear();
    }

    /// <summary>
    /// Clear only selected items from cart (after checkout of selected items)
    /// </summary>
    public void ClearSelectedItems()
    {
        _items.RemoveAll(i => i.IsSelected);
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
