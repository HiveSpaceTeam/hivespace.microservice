namespace HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;

public enum PaymentStatus
{
    Pending,
    Processing,
    Succeeded,
    Failed,
    Cancelled,
    Expired
}
