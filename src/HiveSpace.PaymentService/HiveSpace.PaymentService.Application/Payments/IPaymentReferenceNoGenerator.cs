namespace HiveSpace.PaymentService.Application.Payments;

public interface IPaymentReferenceNoGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
