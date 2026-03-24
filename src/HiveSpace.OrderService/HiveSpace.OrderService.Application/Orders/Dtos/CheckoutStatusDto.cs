namespace HiveSpace.OrderService.Application.Orders.Dtos;

public record CheckoutStatusDto
{
    public Guid    CorrelationId { get; init; }
    public string  CurrentState  { get; init; } = null!;
    public Guid?   OrderId       { get; init; }
    public string? FailureReason { get; init; }
    public bool    IsCompleted   { get; init; }
    public bool    IsFailed      { get; init; }
}
