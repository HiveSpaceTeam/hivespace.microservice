namespace HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;

public record NotifyCustomer
{
    public Guid    CorrelationId        { get; init; }
    public Guid    OrderId              { get; init; }
    public Guid    UserId               { get; init; }
    public bool    IsPartialOrder       { get; init; }
    public int     RejectedPackageCount { get; init; }
    public decimal RefundAmount         { get; init; }
}
