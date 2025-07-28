using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.OrderService.Domain.Events;

public class OrderCreatedEvent : IDomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public double TotalAmount { get; }
    public DateTimeOffset OrderDate { get; }

    public OrderCreatedEvent(Guid orderId, Guid customerId, double totalAmount, DateTimeOffset orderDate)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        OrderDate = orderDate;
    }
}