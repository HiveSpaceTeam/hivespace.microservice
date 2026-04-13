using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Domain.Aggregates.Wallets.Enumerations;

namespace HiveSpace.PaymentService.Domain.Aggregates.Wallets;

public class Transaction : Entity<Guid>
{
    public Guid WalletId { get; private set; }
    public TransactionType Type { get; private set; }
    public TransactionDirection Direction { get; private set; }
    public Money Amount { get; private set; } = null!;
    public Money BalanceAfter { get; private set; } = null!;
    public string Reference { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public DateTimeOffset TransactedAt { get; private set; }

    private Transaction() { }

    internal static Transaction CreateCredit(
        Guid walletId, Money amount, Money balanceAfter, string reference, string description)
        => new()
        {
            Id = Guid.NewGuid(),
            WalletId = walletId,
            Type = DetermineType(reference),
            Direction = TransactionDirection.Credit,
            // Copy Money values to avoid EF Core OwnsOne dual-ownership conflicts.
            // The caller's Money instances (amount, balanceAfter) may already be tracked
            // as owned entities of other aggregates (Payment.Amount, Wallet.AvailableBalance)
            // in the same DbContext. Sharing the same CLR instance across two OwnsOne owners
            // corrupts EF Core's ownership map and causes DbUpdateConcurrencyException.
            Amount = new Money(amount.Amount, amount.Currency),
            BalanceAfter = new Money(balanceAfter.Amount, balanceAfter.Currency),
            Reference = reference,
            Description = description,
            TransactedAt = DateTimeOffset.UtcNow
        };

    internal static Transaction CreateDebit(
        Guid walletId, Money amount, Money balanceAfter, string reference, string description)
        => new()
        {
            Id = Guid.NewGuid(),
            WalletId = walletId,
            Type = DetermineType(reference),
            Direction = TransactionDirection.Debit,
            Amount = new Money(amount.Amount, amount.Currency),
            BalanceAfter = new Money(balanceAfter.Amount, balanceAfter.Currency),
            Reference = reference,
            Description = description,
            TransactedAt = DateTimeOffset.UtcNow
        };

    private static TransactionType DetermineType(string reference) => reference switch
    {
        var r when r.StartsWith("PAYMENT-") => TransactionType.Payment,
        var r when r.StartsWith("REFUND-") => TransactionType.Refund,
        var r when r.StartsWith("WITHDRAWAL-") => TransactionType.Withdrawal,
        var r when r.StartsWith("ESCROW-") => TransactionType.Escrow,
        _ => TransactionType.Adjustment
    };
}
