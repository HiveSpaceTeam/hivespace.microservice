using HiveSpace.OrderService.Infrastructure.Sagas;

namespace HiveSpace.OrderService.Api.Models;

public record CheckoutStatusResponse
{
    public Guid    CorrelationId { get; init; }
    public string  CurrentState  { get; init; }
    public Guid?   OrderId       { get; init; }
    public string? FailureReason { get; init; }
    public bool    IsCompleted   { get; init; }
    public bool    IsFailed      { get; init; }

    public CheckoutStatusResponse(CheckoutSagaState saga)
    {
        CorrelationId = saga.CorrelationId;
        CurrentState  = saga.CurrentState;
        OrderId       = saga.OrderId == default ? null : saga.OrderId;
        FailureReason = saga.FailureReason;
        IsCompleted   = saga.CurrentState == "Completed";
        IsFailed      = saga.CurrentState == "Failed";
    }
}
