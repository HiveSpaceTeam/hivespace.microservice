using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.OrderService.Domain.Enums;

namespace HiveSpace.OrderService.Domain.Events;

public class OrderStatusChangedEvent : IDomainEvent
{
    public Guid OrderId { get; }
    public OrderStatus OldStatus { get; }
    public OrderStatus NewStatus { get; }
    public DateTimeOffset ChangedAt { get; }

    public OrderStatusChangedEvent(Guid orderId, OrderStatus oldStatus, OrderStatus newStatus, DateTimeOffset changedAt)
    {
        OrderId = orderId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedAt = changedAt;
    }
}