using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Domain.Aggregates.Wallets;
using HiveSpace.PaymentService.Domain.Aggregates.Wallets.Enumerations;
using Xunit;

namespace HiveSpace.PaymentService.Tests.Domain;

public class TransactionTests
{
    [Fact]
    public void Create_WithValidFields_SetsAmountAndDirection_Credit()
    {
        var wallet = Wallet.CreateForUser(Guid.NewGuid());
        wallet.Credit(Money.FromVND(100_000), "PAYMENT-001", "payment credit");

        var transaction = wallet.Transactions.Single();
        transaction.Amount.Amount.Should().Be(100_000);
        transaction.Direction.Should().Be(TransactionDirection.Credit);
    }

    [Fact]
    public void Create_WithValidFields_SetsAmountAndDirection_Debit()
    {
        var wallet = Wallet.CreateForUser(Guid.NewGuid());
        wallet.Credit(Money.FromVND(100_000), "PAYMENT-SETUP", "setup");
        wallet.Debit(Money.FromVND(40_000), "WITHDRAWAL-001", "debit test");

        var debit = wallet.Transactions.Single(t => t.Direction == TransactionDirection.Debit);
        debit.Amount.Amount.Should().Be(40_000);
        debit.Direction.Should().Be(TransactionDirection.Debit);
    }

    [Fact]
    public void Create_WithNegativeAmount_ThrowsDomainException()
    {
        var wallet = Wallet.CreateForUser(Guid.NewGuid());
        var act = () => wallet.Credit(Money.FromVND(-1), "REF-NEG", "negative");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_SetsBalanceAfterCorrectly()
    {
        var wallet = Wallet.CreateForUser(Guid.NewGuid());
        wallet.Credit(Money.FromVND(100_000), "PAYMENT-BAL-1", "first credit");
        wallet.Credit(Money.FromVND(50_000), "PAYMENT-BAL-2", "second credit");

        var transactions = wallet.Transactions.ToList();
        transactions[0].BalanceAfter.Amount.Should().Be(100_000);
        transactions[1].BalanceAfter.Amount.Should().Be(150_000);
    }

    [Fact]
    public void Create_DeterminesTypeFromReferencePrefix()
    {
        var wallet = Wallet.CreateForUser(Guid.NewGuid());
        wallet.Credit(Money.FromVND(100_000), "REFUND-001", "refund credit");
        wallet.Credit(Money.FromVND(50_000), "PAYMENT-001", "payment credit");

        var refundTxn = wallet.Transactions.Single(t => t.Reference == "REFUND-001");
        var paymentTxn = wallet.Transactions.Single(t => t.Reference == "PAYMENT-001");

        refundTxn.Type.Should().Be(TransactionType.Refund);
        paymentTxn.Type.Should().Be(TransactionType.Payment);
    }

    [Fact]
    public void Create_WithZeroAmount_ThrowsDomainException()
    {
        var wallet = Wallet.CreateForUser(Guid.NewGuid());
        var act = () => wallet.Debit(Money.Zero(), "DEBIT-ZERO", "zero debit");
        act.Should().Throw<DomainException>();
    }
}
