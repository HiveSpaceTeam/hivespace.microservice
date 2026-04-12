using HiveSpace.PaymentService.Domain.Aggregates.Wallets;
using HiveSpace.PaymentService.Domain.DomainEvents;
using HiveSpace.PaymentService.Domain.Repositories;
using MediatR;

namespace HiveSpace.PaymentService.Application.Wallets.EventHandlers;

// This handler is dispatched by DispatchDomainEventInterceptor.SavingChangesAsync, which runs
// as part of the outer paymentRepository.SaveChangesAsync(). Calling SaveChangesAsync again here
// would create a nested save on the same DbContext — leading to DbUpdateConcurrencyException.
// Instead, mutate the context only; the outer save commits wallet + transaction atomically
// alongside the payment.
//
// Two EF Core pitfalls apply here:
//
// 1. GetByUserIdAsync (no Include) marks the Transactions navigation as IsLoaded=false.
//    DetectChanges skips IsLoaded=false navigations, so any Transaction added to _transactions
//    during this handler would be invisible to EF Core → silent data loss for existing wallets.
//    Fix: use GetByUserIdWithTransactionsAsync so IsLoaded=true and collection additions are tracked.
//
// 2. For new wallets, walletRepository.Add(wallet) calls TrackGraph mid-interceptor. TrackGraph
//    can snapshot owned entities (Transaction.Amount, Transaction.BalanceAfter) in an intermediate
//    state, causing EF Core to generate INSERT + UPDATE instead of a single INSERT → DbUpdateConcurrencyException.
//    Fix: after Add(wallet), call AddTransaction(T) explicitly so EF Core re-registers the
//    Transaction's owned entity entries with a clean Added snapshot via Set<Transaction>().Add().
public class CreditWalletOnPaymentSucceededHandler(IWalletRepository walletRepository)
    : INotificationHandler<PaymentSucceededDomainEvent>
{
    public async Task Handle(PaymentSucceededDomainEvent notification, CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByUserIdWithTransactionsAsync(notification.BuyerId, cancellationToken);
        var isNew = wallet is null;
        if (isNew)
            wallet = Wallet.CreateForUser(notification.BuyerId);

        wallet!.Credit(
            notification.Amount,
            $"PAYMENT-{notification.PaymentId}",
            "Payment received");

        var newTransaction = wallet.Transactions.Last();

        if (isNew)
            walletRepository.Add(wallet);

        // Explicitly register the new Transaction so EF Core tracks it with a clean Added state.
        // For existing wallets this is the primary tracking (collection detection is unreliable
        // mid-interceptor). For new wallets walletRepository.Add already called TrackGraph, but
        // the explicit Add here re-snapshots the owned entities correctly.
        walletRepository.AddTransaction(newTransaction);

        // Do NOT call SaveChangesAsync here.
    }
}
