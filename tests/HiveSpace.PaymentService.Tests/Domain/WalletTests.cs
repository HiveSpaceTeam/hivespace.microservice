using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Domain.Aggregates.Wallets;
using HiveSpace.PaymentService.Domain.Aggregates.Wallets.Enumerations;
using Xunit;

namespace HiveSpace.PaymentService.Tests.Domain;

public class WalletTests
{
    [Fact]
    public void CreateForUser_InitializesWithZeroBalance()
    {
        var wallet = Wallet.CreateForUser(Guid.NewGuid());
        wallet.AvailableBalance.Amount.Should().Be(0);
    }

    [Fact]
    public void Credit_WithPositiveAmount_IncreasesAvailableBalance()
    {
        var wallet = Wallet.CreateForUser(Guid.NewGuid());
        wallet.Credit(Money.FromVND(50_000), "REF-001", "Test credit");
        wallet.AvailableBalance.Amount.Should().Be(50_000);
    }

    [Fact]
    public void Credit_WithZeroAmount_ThrowsDomainException()
    {
        var wallet = Wallet.CreateForUser(Guid.NewGuid());
        var act = () => wallet.Credit(Money.Zero(), "REF-002", "zero amount");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Debit_WithSufficientBalance_DecreasesAvailableBalance()
    {
        var wallet = Wallet.CreateForUser(Guid.NewGuid());
        wallet.Credit(Money.FromVND(100_000), "CREDIT-001", "setup");
        wallet.Debit(Money.FromVND(30_000), "DEBIT-001", "test debit");
        wallet.AvailableBalance.Amount.Should().Be(70_000);
    }

    [Fact]
    public void Debit_WithInsufficientBalance_ThrowsDomainException()
    {
        var wallet = Wallet.CreateForUser(Guid.NewGuid());
        var act = () => wallet.Debit(Money.FromVND(1_000), "DEBIT-002", "no balance");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Debit_OnSuspendedWallet_ThrowsDomainException()
    {
        var wallet = Wallet.CreateForUser(Guid.NewGuid());
        wallet.Credit(Money.FromVND(100_000), "CREDIT-002", "setup");
        wallet.Suspend("test suspension");

        var act = () => wallet.Debit(Money.FromVND(10_000), "DEBIT-003", "suspended wallet");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Suspend_ActiveWallet_SetsStatusToSuspended()
    {
        var wallet = Wallet.CreateForUser(Guid.NewGuid());
        wallet.Suspend("policy violation");
        wallet.Status.Should().Be(WalletStatus.Suspended);
    }

    [Fact]
    public void Activate_SuspendedWallet_SetsStatusToActive()
    {
        var wallet = Wallet.CreateForUser(Guid.NewGuid());
        wallet.Suspend("reason");
        wallet.Activate();
        wallet.Status.Should().Be(WalletStatus.Active);
    }

    [Fact]
    public void CreateForUser_WithEmptyUserId_ThrowsDomainException()
    {
        var act = () => Wallet.CreateForUser(Guid.Empty);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Debit_WithZeroAmount_ThrowsDomainException()
    {
        var wallet = Wallet.CreateForUser(Guid.NewGuid());
        wallet.Credit(Money.FromVND(100_000), "CREDIT-001", "setup");

        var act = () => wallet.Debit(Money.Zero(), "DEBIT-ZERO", "zero amount");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void TotalBalance_EqualsAvailableBalancePlusEscrowBalance()
    {
        var wallet = Wallet.CreateForUser(Guid.NewGuid());
        wallet.Credit(Money.FromVND(75_000), "CREDIT-001", "setup");

        wallet.TotalBalance.Amount.Should().Be(wallet.AvailableBalance.Amount + wallet.EscrowBalance.Amount);
    }
}
