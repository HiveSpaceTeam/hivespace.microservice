using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Exceptions;
using HiveSpace.OrderService.Domain.ValueObjects;

namespace HiveSpace.OrderService.Domain.Aggregates.Orders;

/// <summary>
/// Order aggregate root
/// Contains OrderPackage as entities within the aggregate boundary
/// </summary>
public class Order : AggregateRoot<Guid>, IAuditable
{
    public string ShortId { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public DeliveryAddress DeliveryAddress { get; private set; } = null!;
    public OrderStatus Status { get; private set; } = null!;
    public Money TotalAmount { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public DateTimeOffset? ExpiredAt { get; private set; }

    // OrderPackage entities are INSIDE the aggregate
    private readonly List<OrderPackage> _packages = [];
    public IReadOnlyCollection<OrderPackage> Packages => _packages.AsReadOnly();

    // Audit trail
    private readonly List<OrderTracking> _trackings = [];
    public IReadOnlyCollection<OrderTracking> Trackings => _trackings.AsReadOnly();

    // EF Core constructor
    private Order() { }

    private Order(
        Guid id,
        string shortId,
        Guid userId,
        DeliveryAddress deliveryAddress)
    {
        Id = id;
        ShortId = shortId;
        UserId = userId;
        DeliveryAddress = deliveryAddress;
        Status = OrderStatus.Created;
        TotalAmount = Money.Zero();
        ExpiredAt = DateTimeOffset.UtcNow.AddHours(24);
    }

    public static Order Create(
        Guid userId,
        DeliveryAddress deliveryAddress)
    {
        if (userId == Guid.Empty)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderUserRequired, nameof(userId));
        if (deliveryAddress == null)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderAddressRequired, nameof(deliveryAddress));

        var orderId = Guid.NewGuid();
        var shortId = GenerateShortId();

        var order = new Order(orderId, shortId, userId, deliveryAddress);
        order.AddTracking(OrderTrackingType.Created, ExecutorType.System, null, "Order created");

        return order;
    }

    /// <summary>
    /// Add package as entity within aggregate
    /// </summary>
    public void AddPackage(OrderPackage package)
    {
        if (package is null)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderPackageNull, nameof(package));

        if (Status != OrderStatus.Created)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatus, nameof(Status));

        _packages.Add(package);
        RecalculateTotalAmount();
    }

    /// <summary>
    /// Calculate total from internal packages
    /// Maintains invariant: TotalAmount = sum of all package totals
    /// </summary>
    private void RecalculateTotalAmount()
    {
        TotalAmount = Money.Sum(_packages.Select(p => p.TotalAmount));
    }

    public void MarkAsPaid(Guid paymentId)
    {
        if (Status != OrderStatus.Created)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatusForPayment, nameof(Status));

        Status = OrderStatus.Paid;
        PaidAt = DateTimeOffset.UtcNow;

        AddTracking(OrderTrackingType.Paid, ExecutorType.System, null, $"Order paid via payment {paymentId}");
        // Domain event: OrderPaidDomainEvent
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
        // Domain event: OrderMarkedAsCODDomainEvent
    }

    /// <summary>
    /// Confirms order when all packages are confirmed
    /// </summary>
    public void Confirm()
    {
        if (Status != OrderStatus.Paid && Status != OrderStatus.COD)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatusForConfirmation, nameof(Status));

        // Check all packages are confirmed
        if (!_packages.All(p => p.Status == OrderPackageStatus.Confirmed))
            throw new InvalidFieldException(OrderDomainErrorCode.OrderPackagesNotConfirmed, nameof(Packages));

        Status = OrderStatus.Confirmed;
        AddTracking(OrderTrackingType.Confirmed, ExecutorType.System, null, "All packages confirmed");
        // Domain event: OrderConfirmedDomainEvent
    }

    /// <summary>
    /// Update specific package within aggregate
    /// </summary>
    public void ConfirmPackage(Guid packageId, Guid confirmedBy)
    {
        var package = _packages.FirstOrDefault(p => p.Id == packageId) ?? 
            throw new NotFoundException(OrderDomainErrorCode.OrderPackageNotFound, nameof(packageId));
        package.Confirm(confirmedBy);

        AddTracking(OrderTrackingType.PackageConfirmed, ExecutorType.User, confirmedBy, $"Package {packageId} confirmed");

        // Check if all packages are confirmed
        if (_packages.All(p => p.Status == OrderPackageStatus.Confirmed || p.Status == OrderPackageStatus.Rejected))
        {
            // Only proceed if at least one package is confirmed
            if (_packages.Any(p => p.Status == OrderPackageStatus.Confirmed))
            {
                Confirm();
            }
        }
    }

    /// <summary>
    /// Reject specific package within aggregate
    /// </summary>
    public void RejectPackage(Guid packageId, string reason, Guid rejectedBy)
    {
        var package = _packages.FirstOrDefault(p => p.Id == packageId) ?? 
            throw new NotFoundException(OrderDomainErrorCode.OrderPackageNotFound, nameof(packageId));
        package.Reject(reason, rejectedBy);

        AddTracking(OrderTrackingType.PackageRejected, ExecutorType.User, rejectedBy, $"Package {packageId} rejected: {reason}");

        // Recalculate total excluding rejected packages
        TotalAmount = Money.Sum(_packages
            .Where(p => p.Status != OrderPackageStatus.Rejected)
            .Select(p => p.TotalAmount));

        // Check if all packages are rejected
        if (_packages.All(p => p.Status == OrderPackageStatus.Rejected))
        {
            Cancel("All packages rejected", UserId);
        }
    }

    /// <summary>
    /// Assign shipping to package within aggregate
    /// </summary>
    public void AssignShippingToPackage(Guid packageId, Guid shippingId)
    {
        var package = _packages.FirstOrDefault(p => p.Id == packageId) ?? 
            throw new NotFoundException(OrderDomainErrorCode.OrderPackageNotFound, nameof(packageId));
        package.AssignShipping(shippingId);
    }

    /// <summary>
    /// Mark package as delivered (triggered by shipping event)
    /// </summary>
    public void MarkPackageAsDelivered(Guid packageId)
    {
        var package = _packages.FirstOrDefault(p => p.Id == packageId) ?? 
            throw new NotFoundException(OrderDomainErrorCode.OrderPackageNotFound, nameof(packageId));
        package.MarkAsDelivered();

        AddTracking(OrderTrackingType.PackageDelivered, ExecutorType.System, null, $"Package {packageId} delivered");

        // Check if all packages are delivered
        if (_packages.All(p => p.Status == OrderPackageStatus.Delivered || p.Status == OrderPackageStatus.Rejected))
        {
            Status = OrderStatus.Delivered;
            // Domain event: OrderDeliveredDomainEvent
        }
    }

    public void Complete()
    {
        if (Status != OrderStatus.Delivered)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatusForCompletion, nameof(Status));

        // Complete all packages
        foreach (var package in _packages.Where(p => p.Status == OrderPackageStatus.Delivered))
        {
            package.Complete();
        }

        Status = OrderStatus.Completed;
        AddTracking(OrderTrackingType.Completed, ExecutorType.System, null, "Order completed");
        // Domain event: OrderCompletedDomainEvent
    }

    public void Cancel(string reason, Guid cancelledBy)
    {
        if (!Status.CanBeCancelled())
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatusForCancellation, nameof(Status));

        var previousStatus = Status;

        // Cancel all active packages
        foreach (var package in _packages.Where(p => p.Status.CanCancel()))
        {
            package.Cancel(reason, cancelledBy);
        }

        Status = OrderStatus.Cancelled;
        AddTracking(OrderTrackingType.Cancelled, ExecutorType.User, cancelledBy, $"Order cancelled: {reason}");
        // Domain event: OrderCancelledDomainEvent
    }

    public void MarkAsExpired()
    {
        if (Status != OrderStatus.Created)
            throw new InvalidFieldException(OrderDomainErrorCode.OrderInvalidStatusForExpiration, nameof(Status));

        Status = OrderStatus.Expired;
        AddTracking(OrderTrackingType.Expired, ExecutorType.System, null, "Order payment expired");
        // Domain event: OrderExpiredDomainEvent
    }

    private void AddTracking(string type, ExecutorType executorType, Guid? executorId, string message)
    {
        var tracking = OrderTracking.Create(type, executorType, executorId, message);
        _trackings.Add(tracking);
    }

    private static string GenerateShortId()
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"ORD-{timestamp}-{random}";
    }
}
 