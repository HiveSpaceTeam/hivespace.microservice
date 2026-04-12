using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;

namespace HiveSpace.PaymentService.Domain.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken ct = default);
    Task<Payment?> GetByIdempotencyKeyAsync(string key, CancellationToken ct = default);
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
}
