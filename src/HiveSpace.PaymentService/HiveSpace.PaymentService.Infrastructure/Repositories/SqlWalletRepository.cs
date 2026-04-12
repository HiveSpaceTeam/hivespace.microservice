using HiveSpace.Infrastructure.Persistence.Repositories;
using HiveSpace.PaymentService.Domain.Aggregates.Wallets;
using HiveSpace.PaymentService.Domain.Repositories;
using HiveSpace.PaymentService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.PaymentService.Infrastructure.Repositories;

public class SqlWalletRepository(PaymentDbContext db)
    : BaseRepository<Wallet, Guid>(db), IWalletRepository
{
    public async Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, ct);

    public async Task<Wallet?> GetByUserIdWithTransactionsAsync(Guid userId, CancellationToken ct = default)
        => await db.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.UserId == userId, ct);

    public void AddTransaction(Transaction transaction)
        => db.Set<Transaction>().Add(transaction);
}
