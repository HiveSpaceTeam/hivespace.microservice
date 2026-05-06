namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record OrderTrackingDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = null!;
    public string ExecutorType { get; init; } = null!;
    public Guid? ExecutorId { get; init; }
    public string Message { get; init; } = null!;
    public DateTimeOffset CreatedAt { get; init; }
}
