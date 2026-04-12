using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Domain.Aggregates.Wallets.Enumerations;
using HiveSpace.PaymentService.Domain.DomainEvents;
using HiveSpace.PaymentService.Domain.Exceptions;

namespace HiveSpace.PaymentService.Domain.Aggregates.Wallets;

public class Wallet : AggregateRoot<Guid>, IAuditable
{
    private readonly List<Transaction> _transactions = [];

    public Guid UserId { get; private set; }
    public Money AvailableBalance { get; private set; } = null!;
    public Money EscrowBalance { get; private set; } = null!;     // Phase 2 — always 0 in Phase 1
    public Money TotalBalance => AvailableBalance + EscrowBalance;
    public int RewardPoints { get; private set; }
    public WalletStatus Status { get; private set; }
    public string? SuspensionReason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    private Wallet() { }

    public static Wallet CreateForUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new InvalidFieldException(PaymentDomainErrorCode.WalletUserIdRequired, nameof(userId));

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AvailableBalance = Money.Zero(),
            EscrowBalance = Money.Zero(),
            RewardPoints = 0,
            Status = WalletStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        wallet.AddDomainEvent(new WalletCreatedDomainEvent(wallet.Id, wallet.UserId));

        return wallet;
    }

    public void Credit(Money amount, string reference, string description)
    {
        EnsureActive();
        AvailableBalance += amount;
        _transactions.Add(Transaction.CreateCredit(Id, amount, AvailableBalance, reference, description));
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new WalletCreditedDomainEvent(Id, UserId, amount, reference));
    }

    public void Debit(Money amount, string reference, string description)
    {
        EnsureActive();
        if (AvailableBalance < amount)
            throw new InvalidFieldException(PaymentDomainErrorCode.WalletInsufficientBalance, nameof(amount));

        AvailableBalance -= amount;
        _transactions.Add(Transaction.CreateDebit(Id, amount, AvailableBalance, reference, description));
        UpdatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new WalletDebitedDomainEvent(Id, UserId, amount, reference));
    }

    public void Suspend(string reason)
    {
        Status = WalletStatus.Suspended;
        SuspensionReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        Status = WalletStatus.Active;
        SuspensionReason = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void EnsureActive()
    {
        if (Status != WalletStatus.Active)
            throw new InvalidFieldException(PaymentDomainErrorCode.WalletInactive, nameof(Status));
    }
}
