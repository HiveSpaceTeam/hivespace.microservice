using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.IdGeneration;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.ValueObjects;

namespace HiveSpace.OrderService.Domain.Aggregates.Orders;

public class Order : AggregateRoot<Guid>, IAuditable
{
    public string ShortId { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public Guid StoreId { get; private set; }
    public DeliveryAddress DeliveryAddress { get; private set; } = null!;
    public OrderStatus Status { get; private set; } = null!;

    // Financial breakdown
    public Money SubTotal { get; private set; } = null!;
    public Money TotalDiscount { get; private set; } = null!;
    public Money ShippingFee { get; private set; } = null!;
    public Money TotalAmount { get; private set; } = null!;
    public bool IsShippingPaidBySeller { get; private set; }

    // Shipping reference
    public Guid? ShippingId { get; private set; }

    // Rejection
    public string? RejectionReason { get; private set; }

    // Timestamps
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset? RejectedAt { get; private set; }
    public DateTimeOffset? ExpiredAt { get; private set; }

    // Items owned by this order
    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // Payment breakdown (value objects)
    private readonly List<Checkout> _checkouts = [];
    public IReadOnlyCollection<Checkout> Checkouts => _checkouts.AsReadOnly();

    // Applied discounts (value objects)
    private readonly List<Discount> _discounts = [];
    public IReadOnlyCollection<Discount> Discounts => _discounts.AsReadOnly();

    // Audit trail
    private readonly List<OrderTracking> _trackings = [];
    public IReadOnlyCollection<OrderTracking> Trackings => _trackings.AsReadOnly();

    // EF Core constructor
    private Order() { }

    private Order(
        Guid id,
        string shortId,
        Guid userId,
        Guid storeId,
        DeliveryAddress deliveryAddress)
    {
        Id = id;
        ShortId = shortId;
        UserId = userId;
        StoreId = storeId;
        DeliveryAddress = deliveryAddress;
        Status = OrderStatus.Created;
        SubTotal = Money.Zero();
        TotalDiscount = Money.Zero();
        ShippingFee = Money.Zero();
        TotalAmount = Money.Zero();
        IsShippingPaidBySeller = false;
        CreatedAt = DateTimeOffset.UtcNow;
        ExpiredAt = DateTimeOffset.UtcNow.AddHours(24);
    }

    public static Order Create(
        Guid userId,
        DeliveryAddress deliveryAddress,
        Guid storeId,
        Guid? id = null)
    {
        if (userId == Guid.Empty)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderUserRequired, nameof(userId));
        if (deliveryAddress == null)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderAddressRequired, nameof(deliveryAddress));
        if (storeId == Guid.Empty)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderStoreIdRequired, nameof(storeId));

        var orderId = (id.HasValue && id.Value != Guid.Empty) ? id.Value : IdGenerator.NewId<Guid>();
        var shortId = GenerateShortId();

        var order = new Order(orderId, shortId, userId, storeId, deliveryAddress);
        order.AddTracking(OrderTrackingType.Created, ExecutorType.System, null, "Order created");

        return order;
    }

    // ── Item management ───────────────────────────────────────────────────────

    public void AddItem(
        long productId,
        long skuId,
        int quantity,
        Money unitPrice,
        ProductSnapshot productSnapshot,
        bool isCOD = false)
    {
        if (Status != OrderStatus.Created)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatus, nameof(Status));

        EnsureNoAppliedDiscounts();

        var item = OrderItem.Create(productId, skuId, quantity, unitPrice, productSnapshot, isCOD);
        _items.Add(item);
        RecalculateTotals();
    }

    public void SetShippingFee(Money shippingFee, bool isShippingPaidBySeller)
    {
        if (shippingFee == null || shippingFee.Amount < 0)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidShippingFee, nameof(shippingFee));

        EnsureNoAppliedDiscounts();

        ShippingFee = shippingFee;
        IsShippingPaidBySeller = isShippingPaidBySeller;
        RecalculateTotals();
    }

    public void AddCheckout(PaymentMethod paymentMethod, Money amount)
    {
        _checkouts.Add(Checkout.Create(paymentMethod, amount));
    }

    public void ApplyDiscount(Coupon coupon)
    {
        if (Status != OrderStatus.Created)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatus, nameof(Status));

        if (coupon is null)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalid, nameof(coupon));

        if (coupon.OwnerType == CouponOwnerType.Store && coupon.StoreId != StoreId)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponStoreNotApplicable, nameof(coupon.StoreId));

        var discountAmount = coupon.CalculateDiscount(SubTotal);

        var discount = coupon.OwnerType == CouponOwnerType.Store
            ? Discount.CreateStoreDiscount(coupon.Id, coupon.Code, discountAmount, coupon.Scope)
            : Discount.CreatePlatformDiscount(coupon.Id, coupon.Code, discountAmount, coupon.Scope);

        _discounts.Add(discount);
        RecalculateTotals();
    }

    public void ApplyProratedDiscount(Coupon coupon, Money discountAmount)
    {
        if (Status != OrderStatus.Created)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatus, nameof(Status));

        if (coupon is null)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalid, nameof(coupon));

        if (coupon.OwnerType != CouponOwnerType.Platform)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalid, nameof(coupon.OwnerType));

        if (discountAmount is null || discountAmount.Amount <= 0)
            throw new InvalidFieldException(OrderDomainErrorCode.CouponInvalidDiscountAmount, nameof(discountAmount));

        var discount = coupon.OwnerType == CouponOwnerType.Store
            ? Discount.CreateStoreDiscount(coupon.Id, coupon.Code, discountAmount, coupon.Scope)
            : Discount.CreatePlatformDiscount(coupon.Id, coupon.Code, discountAmount, coupon.Scope);

        _discounts.Add(discount);
        RecalculateTotals();
    }

    // ── Payment ───────────────────────────────────────────────────────────────

    public void MarkAsPaid(Guid paymentId)
    {
        if (Status == OrderStatus.Paid) return;

        if (Status != OrderStatus.Created)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatusForPayment, nameof(Status));

        Status = OrderStatus.Paid;
        PaidAt = DateTimeOffset.UtcNow;
        AddTracking(OrderTrackingType.Paid, ExecutorType.System, null, $"Order paid via payment {paymentId}");
    }

    public void MarkAsCOD()
    {
        if (Status != OrderStatus.Created)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatusForCOD, nameof(Status));

        if (TotalAmount.ExceedsCODLimit())
            throw new InvalidFieldException(OrderDomainErrorCode.OrderExceedsCODLimit, nameof(TotalAmount));

        Status = OrderStatus.COD;
        PaidAt = DateTimeOffset.UtcNow;
        AddTracking(OrderTrackingType.COD, ExecutorType.System, null, "Order marked as COD");
    }

    // ── Seller confirm / reject ───────────────────────────────────────────────

    public void Confirm(Guid confirmedBy)
    {
        if (confirmedBy == Guid.Empty)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidExecutorId, nameof(confirmedBy));

        if (!Status.CanBeConfirmed())
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatusForConfirmation, nameof(Status));

        if (_items.Count == 0)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderNoItems, nameof(_items));

        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTimeOffset.UtcNow;
        AddTracking(OrderTrackingType.Confirmed, ExecutorType.User, confirmedBy, "Order confirmed by seller");
    }

    public void Reject(string reason, Guid rejectedBy)
    {
        if (rejectedBy == Guid.Empty)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidExecutorId, nameof(rejectedBy));

        if (!Status.CanBeRejected())
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatusForRejection, nameof(Status));

        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidFieldException(OrderDomainErrorCode.OrderRejectionReasonRequired, nameof(reason));

        Status = OrderStatus.Rejected;
        RejectionReason = reason;
        RejectedAt = DateTimeOffset.UtcNow;
        AddTracking(OrderTrackingType.Rejected, ExecutorType.User, rejectedBy, $"Order rejected: {reason}");
    }

    // ── Shipping lifecycle ────────────────────────────────────────────────────

    public void AssignShipping(Guid shippingId)
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatusForShipping, nameof(Status));

        if (shippingId == Guid.Empty)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderShippingIdRequired, nameof(shippingId));

        ShippingId = shippingId;
        Status = OrderStatus.ReadyToShip;
    }

    public void Ship()
    {
        if (!Status.CanBeShipped())
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatusForShipping, nameof(Status));

        if (ShippingId == null)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderMissingShipping, nameof(ShippingId));

        Status = OrderStatus.Shipped;
        AddTracking(OrderTrackingType.Shipped, ExecutorType.System, null, "Order shipped");
    }

    public void MarkAsDelivered()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatusForDelivery, nameof(Status));

        Status = OrderStatus.Delivered;
        AddTracking(OrderTrackingType.Delivered, ExecutorType.System, null, "Order delivered");
    }

    public void Complete()
    {
        if (Status != OrderStatus.Delivered)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatusForCompletion, nameof(Status));

        Status = OrderStatus.Completed;
        AddTracking(OrderTrackingType.Completed, ExecutorType.System, null, "Order completed");
    }

    public void Cancel(string reason, Guid cancelledBy)
    {
        if (!Status.CanBeCancelled())
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatusForCancellation, nameof(Status));

        Status = OrderStatus.Cancelled;
        AddTracking(OrderTrackingType.Cancelled, ExecutorType.User, cancelledBy, $"Order cancelled: {reason}");
    }

    public void MarkAsExpired()
    {
        if (Status != OrderStatus.Created)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatusForExpiration, nameof(Status));

        Status = OrderStatus.Expired;
        AddTracking(OrderTrackingType.Expired, ExecutorType.System, null, "Order payment expired");
    }

    // ── Financial helpers ─────────────────────────────────────────────────────

    public Money GetCODAmount()
    {
        return Money.Sum(_items.Where(i => i.IsCOD).Select(i => i.LineTotal));
    }

    public bool HasCODItems() => _items.Any(i => i.IsCOD);

    public Money CalculateSellerPayout()
    {
        const decimal SERVICE_FEE_RATE = 0.099m;

        var storeDiscountTotal = _discounts
            .Where(d => d.CouponOwnerType == CouponOwnerType.Store)
            .Aggregate(Money.Zero(SubTotal.Currency), (acc, d) => acc + d.DiscountAmount);

        var sellerBase = SubTotal - storeDiscountTotal;
        var serviceFee = sellerBase.CalculateServiceFee(SERVICE_FEE_RATE);
        var payout = sellerBase - serviceFee;

        if (IsShippingPaidBySeller)
            payout -= ShippingFee;

        return payout;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void RecalculateTotals()
    {
        SubTotal = Money.Sum(_items.Select(i => i.LineTotal));
        TotalDiscount = Money.Sum(_discounts.Select(d => d.DiscountAmount));

        var buyerShippingFee = IsShippingPaidBySeller ? Money.Zero() : ShippingFee;
        TotalAmount = SubTotal.ApplyDiscount(TotalDiscount) + buyerShippingFee;
    }

    private void EnsureNoAppliedDiscounts()
    {
        if (_discounts.Count > 0)
            throw new InvalidFieldException(OrderDomainErrorCode.DiscountAlreadyApplied, nameof(_discounts));
    }

    private void AddTracking(string type, ExecutorType executorType, Guid? executorId, string message)
    {
        _trackings.Add(OrderTracking.Create(type, executorType, executorId, message));
    }

    private static string GenerateShortId()
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Random.Shared.Next(100_000, 999_999);
        return $"ORD-{timestamp}-{random}";
    }
}
