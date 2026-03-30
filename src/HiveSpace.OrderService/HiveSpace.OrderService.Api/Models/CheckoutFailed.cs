namespace HiveSpace.OrderService.Api.Models;

public record CheckoutFailed
{
    public Guid              CorrelationId { get; init; }
    public string            Reason        { get; init; } = null!;
    public List<string>      Errors        { get; init; } = new();
    public CheckoutErrorType ErrorType     { get; init; }
}
