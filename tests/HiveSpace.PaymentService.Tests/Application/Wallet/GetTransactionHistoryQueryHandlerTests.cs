using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Application.Wallets.Queries.GetTransactionHistory;
using HiveSpace.PaymentService.Infrastructure.Repositories;
using HiveSpace.PaymentService.Tests.Fixtures;
using Xunit;
using WalletAggregate = HiveSpace.PaymentService.Domain.Aggregates.Wallets.Wallet;

namespace HiveSpace.PaymentService.Tests.Application.Wallet;

public class GetTransactionHistoryQueryHandlerTests : IClassFixture<PaymentServiceFixture>
{
    private readonly PaymentServiceFixture _fixture;

    public GetTransactionHistoryQueryHandlerTests(PaymentServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithTransactions_ReturnsAllItems()
    {
        var userId = Guid.NewGuid();
        var wallet = WalletAggregate.CreateForUser(userId);
        wallet.Credit(Money.FromVND(50_000), "REFUND-TH-1", "First credit");
        wallet.Credit(Money.FromVND(30_000), "REFUND-TH-2", "Second credit");
        _fixture.DbContext.Wallets.Add(wallet);
        await _fixture.DbContext.SaveChangesAsync();

        var result = await BuildHandler().Handle(
            new GetTransactionHistoryQuery(userId, Page: 1, PageSize: 10),
            CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Pagination.TotalItems.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithPagination_RespectsPageSize()
    {
        var userId = Guid.NewGuid();
        var wallet = WalletAggregate.CreateForUser(userId);
        wallet.Credit(Money.FromVND(10_000), "REFUND-TH-P1", "credit 1");
        wallet.Credit(Money.FromVND(20_000), "REFUND-TH-P2", "credit 2");
        wallet.Credit(Money.FromVND(30_000), "REFUND-TH-P3", "credit 3");
        _fixture.DbContext.Wallets.Add(wallet);
        await _fixture.DbContext.SaveChangesAsync();

        var result = await BuildHandler().Handle(
            new GetTransactionHistoryQuery(userId, Page: 1, PageSize: 2),
            CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Pagination.TotalItems.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WhenWalletNotFound_ThrowsNotFoundException()
    {
        var act = () => BuildHandler().Handle(
            new GetTransactionHistoryQuery(Guid.NewGuid(), Page: 1, PageSize: 10),
            CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    private GetTransactionHistoryQueryHandler BuildHandler() =>
        new(new SqlWalletRepository(_fixture.DbContext));
}
