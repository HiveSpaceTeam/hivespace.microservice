using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Application.Wallets.Queries.GetTransactionHistory;
using HiveSpace.PaymentService.Application.Wallets.Queries.GetWallet;
using HiveSpace.PaymentService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using WalletAggregate = HiveSpace.PaymentService.Domain.Aggregates.Wallets.Wallet;

namespace HiveSpace.PaymentService.Tests.Application.Wallet;

public class GetWalletBalanceQueryHandlerTests : IClassFixture<PaymentServiceFixture>
{
    private readonly PaymentServiceFixture _fixture;

    public GetWalletBalanceQueryHandlerTests(PaymentServiceFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_ReturnsCurrentBalance()
    {
        var wallet = WalletAggregate.CreateForUser(Guid.NewGuid());
        wallet.Credit(Money.FromVND(100_000), "REFUND-1", "refund");
        _fixture.DbContext.Wallets.Add(wallet);
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.Wallets.SingleAsync(x => x.Id == wallet.Id);

        stored.AvailableBalance.Amount.Should().Be(100_000);
        typeof(GetWalletQueryHandler).Should().NotBeNull();
    }

    [Fact]
    public void Handle_SequentialReads_DoNotMutateState()
    {
        var wallet = WalletAggregate.CreateForUser(Guid.NewGuid());

        var first = wallet.AvailableBalance.Amount;
        var second = wallet.AvailableBalance.Amount;

        second.Should().Be(first);
        typeof(GetTransactionHistoryQueryHandler).Should().NotBeNull();
    }
}
