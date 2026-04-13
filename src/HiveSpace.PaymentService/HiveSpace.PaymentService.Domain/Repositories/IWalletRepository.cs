using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.PaymentService.Domain.Aggregates.Wallets;

namespace HiveSpace.PaymentService.Domain.Repositories;

public interface IWalletRepository : IRepository<Wallet>
{
    Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Wallet?> GetByUserIdWithTransactionsAsync(Guid userId, CancellationToken ct = default);
    void AddTransaction(Transaction transaction);
}
