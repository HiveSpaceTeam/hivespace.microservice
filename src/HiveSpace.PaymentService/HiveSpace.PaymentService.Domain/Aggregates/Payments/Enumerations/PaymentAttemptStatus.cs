namespace HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;

public enum PaymentAttemptStatus
{
    Pending,
    Processing,
    Succeeded,
    Failed,
    Expired,
    Cancelled
}
