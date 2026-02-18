using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.ValueObjects;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;

namespace HiveSpace.OrderService.Domain.Aggregates.Orders;

/// <summary>
/// OrderPackage - Entity within Order aggregate (NOT an aggregate root)
/// Represents items from the same store in a multi-vendor order
/// Lifecycle is completely dependent on parent Order
/// </summary>
public class OrderPackage : Entity<Guid>, IAuditable
{
    public Guid StoreId { get; private set; }
    public Guid BuyerId { get; private set; }
    public OrderPackageStatus Status { get; private set; } = null!;
    public Money SubTotal { get; private set; } = null!;
    public Money TotalDiscount { get; private set; } = null!;
    public Money ShippingFee { get; private set; } = null!;
    public Money TotalAmount { get; private set; } = null!;
    public bool IsShippingPaidBySeller { get; private set; } 
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset? RejectedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    // Reference to external Shipping aggregate by ID only
    public Guid? ShippingId { get; private set; }

    // Entities within this entity
    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private readonly List<Checkout> _checkouts = [];
    public IReadOnlyCollection<Checkout> Checkouts => _checkouts.AsReadOnly();

    private readonly List<Discount> _discounts = [];
    public IReadOnlyCollection<Discount> Discounts => _discounts.AsReadOnly();

    // EF Core constructor
    private OrderPackage() { }

    private OrderPackage(
        Guid storeId,
        Guid buyerId)
    {
        Id = Guid.NewGuid();
        StoreId = storeId;
        BuyerId = buyerId;
        Status = OrderPackageStatus.Pending;
        SubTotal = Money.Zero();
        TotalDiscount = Money.Zero();
        ShippingFee = Money.Zero();
        TotalAmount = Money.Zero();
        IsShippingPaidBySeller = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Factory method - no OrderId parameter needed (it's inside Order)
    /// </summary>
    public static OrderPackage Create(
        Guid storeId,
        Guid buyerId)
    {
        if (storeId == Guid.Empty)
            throw new DomainException(400, OrderDomainErrorCode.PackageStoreIdRequired, nameof(storeId));
        if (buyerId == Guid.Empty)
            throw new DomainException(400, OrderDomainErrorCode.PackageBuyerIdRequired, nameof(buyerId));

        return new OrderPackage(storeId, buyerId);
    }

    public void AddItem(
        Guid productId,
        Guid skuId,
        int quantity,
        Money unitPrice,
        ProductSnapshot productSnapshot,
        bool isCOD = false)
    {
        if (Status != OrderPackageStatus.Pending)
            throw new DomainException(400, OrderDomainErrorCode.PackageInvalidStatusForAddItem, nameof(Status));

        var item = OrderItem.Create(productId, skuId, quantity, unitPrice, productSnapshot, isCOD);
        _items.Add(item);

        RecalculateTotals();
    }

    public void ApplyDiscount(Coupon coupon)
    {
        if (Status != OrderPackageStatus.Pending)
            throw new InvalidFieldException(OrderDomainErrorCode.PackageInvalidStatusForDiscount, nameof(Status));

        var discountAmount = coupon.CalculateDiscount(SubTotal);
        
        Discount discountEntity;
        
        if (coupon.OwnerType == CouponOwnerType.Store)
        {
            // Store bears the cost
            discountEntity = Discount.CreateStoreDiscount(
                coupon.Id, 
                coupon.Code, 
                discountAmount, 
                coupon.Scope
            );
        }
        else // Platform coupon
        {
            // Platform subsidizes
            discountEntity = Discount.CreatePlatformDiscount(
                coupon.Id, 
                coupon.Code, 
                discountAmount, 
                coupon.Scope
            );
        }
        
        _discounts.Add(discountEntity);

        RecalculateTotals();
    }

    public void SetShippingFee(Money shippingFee, bool isShippingPaidBySeller)
    {
        if (shippingFee == null || shippingFee.Amount < 0)
            throw new DomainException(400, OrderDomainErrorCode.PackageInvalidShippingFee, nameof(shippingFee));

        ShippingFee = shippingFee;
        IsShippingPaidBySeller = isShippingPaidBySeller;

        RecalculateTotals();
    }

    public void AddCheckout(PaymentMethod paymentMethod, Money amount)
    {
        var checkout = Checkout.Create(paymentMethod, amount);
        _checkouts.Add(checkout);
    }

    /// <summary>
    /// Seller confirms the package
    /// </summary>
    public void Confirm(Guid confirmedBy)
    {
        if (!Status.CanConfirm())
            throw new DomainException(400, OrderDomainErrorCode.PackageInvalidStatusForConfirmation, nameof(Status));

        if (_items.Count == 0)
            throw new DomainException(400, OrderDomainErrorCode.PackageNoItems, nameof(_items));

        Status = OrderPackageStatus.Confirmed;
        ConfirmedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Seller rejects the package
    /// </summary>
    public void Reject(string reason, Guid rejectedBy)
    {
        if (!Status.CanReject())
            throw new DomainException(400, OrderDomainErrorCode.PackageInvalidStatusForRejection, nameof(Status));

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException(400, OrderDomainErrorCode.PackageRejectionReasonRequired, nameof(reason));

        Status = OrderPackageStatus.Rejected;
        RejectedAt = DateTimeOffset.UtcNow;
        RejectionReason = reason;
    }

    /// <summary>
    /// Assign shipping (reference to separate Shipping aggregate)
    /// </summary>
    public void AssignShipping(Guid shippingId)
    {
        if (Status != OrderPackageStatus.Confirmed)
            throw new DomainException(400, OrderDomainErrorCode.PackageInvalidStatusForShipping, nameof(Status));

        if (shippingId == Guid.Empty)
            throw new DomainException(400, OrderDomainErrorCode.PackageShippingIdRequired, nameof(shippingId));

        ShippingId = shippingId;
        Status = OrderPackageStatus.ReadyToShip;
    }

    public void Ship()
    {
        if (!Status.CanShip())
            throw new DomainException(400, OrderDomainErrorCode.PackageInvalidStatusForShipping, nameof(Status));

        if (ShippingId == null)
            throw new DomainException(400, OrderDomainErrorCode.PackageMissingShipping, nameof(ShippingId));

        Status = OrderPackageStatus.Shipped;
    }

    public void MarkAsDelivered()
    {
        if (Status != OrderPackageStatus.Shipped)
            throw new DomainException(400, OrderDomainErrorCode.PackageInvalidStatusForDelivery, nameof(Status));

        Status = OrderPackageStatus.Delivered;
    }

    public void Complete()
    {
        if (Status != OrderPackageStatus.Delivered)
            throw new DomainException(400, OrderDomainErrorCode.PackageInvalidStatusForCompletion, nameof(Status));

        Status = OrderPackageStatus.Completed;
    }

    public void Cancel(string reason, Guid cancelledBy)
    {
        if (!Status.CanCancel())
            throw new DomainException(400, OrderDomainErrorCode.PackageInvalidStatusForCancellation, nameof(Status));

        Status = OrderPackageStatus.Cancelled;
    }

    /// <summary>
    /// Calculates seller payout after service fee deduction
    /// </summary>
    public Money CalculateSellerPayout()
    {
        const decimal SERVICE_FEE_RATE = 0.099m; // 9.9%
        
        var serviceFee = SubTotal.CalculateServiceFee(SERVICE_FEE_RATE);
        var payout = SubTotal - serviceFee;

        if (IsShippingPaidBySeller)
            payout -= ShippingFee;

        return payout;
    }

    public Money GetCODAmount()
    {
        var codItems = _items.Where(i => i.IsCOD);
        return Money.Sum(codItems.Select(i => i.LineTotal));
    }

    public bool HasCODItems() => _items.Any(i => i.IsCOD);

    private void RecalculateTotals()
    {
        SubTotal = Money.Sum(_items.Select(i => i.LineTotal));
        TotalDiscount = Money.Sum(_discounts.Select(d => d.DiscountAmount));
        
        var buyerShippingFee = IsShippingPaidBySeller ? Money.Zero() : ShippingFee;
        TotalAmount = SubTotal - TotalDiscount + buyerShippingFee;
    }
}
