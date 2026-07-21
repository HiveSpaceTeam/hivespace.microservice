using HiveSpace.Infrastructure.Persistence.Repositories;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Repositories;
using HiveSpace.PaymentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.PaymentService.Infrastructure.Repositories;

public class SqlPaymentRepository(PaymentDbContext db)
    : BaseRepository<Payment, Guid>(db), IPaymentRepository
{
    public async Task<Payment?> GetByIdAsync(Guid paymentId, CancellationToken ct = default)
        => await db.Payments
            .Include(p => p.LinkedOrders)
            .Include(p => p.Attempts)
            .FirstOrDefaultAsync(p => p.Id == paymentId, ct);

    public async Task<Payment?> GetByIdempotencyKeyAsync(string key, CancellationToken ct = default)
        => await db.Payments
            .Include(p => p.LinkedOrders)
            .Include(p => p.Attempts)
            .FirstOrDefaultAsync(p => p.IdempotencyKey == key, ct);

    public async Task<Payment?> GetByReferenceNoAsync(string referenceNo, CancellationToken ct = default)
        => await db.Payments
            .Include(p => p.LinkedOrders)
            .Include(p => p.Attempts)
            .FirstOrDefaultAsync(p => p.ReferenceNo == referenceNo, ct);

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        => await db.Payments
            .Include(p => p.LinkedOrders)
            .Include(p => p.Attempts)
            .FirstOrDefaultAsync(p => p.OrderId == orderId || p.LinkedOrders.Any(o => o.OrderId == orderId), ct);

    public async Task<bool> ReferenceNoExistsAsync(string referenceNo, CancellationToken ct = default)
        => await db.Payments.AnyAsync(p => p.ReferenceNo == referenceNo, ct);
}
