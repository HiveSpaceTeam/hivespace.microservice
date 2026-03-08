using HiveSpace.Domain.Shared.Entities;
using HiveSpace.OrderService.Domain.Enumerations;

namespace HiveSpace.OrderService.Domain.Aggregates.Orders;

public class OrderTracking : Entity<Guid>
{
    public string Type { get; private set; } = null!;
    public ExecutorType ExecutorType { get; private set; } = null!;
    public Guid? ExecutorId { get; private set; }
    public string Message { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    private OrderTracking() { }

    public static OrderTracking Create(
        string type,
        ExecutorType executorType,
        Guid? executorId,
        string message)
    {
        return new OrderTracking
        {
            Id = Guid.NewGuid(),
            Type = type,
            ExecutorType = executorType,
            ExecutorId = executorId,
            Message = message,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
