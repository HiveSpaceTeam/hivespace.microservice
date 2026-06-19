using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Application.Wallets.Queries.GetWallet;
using HiveSpace.PaymentService.Infrastructure.Repositories;
using HiveSpace.PaymentService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;
using WalletAggregate = HiveSpace.PaymentService.Domain.Aggregates.Wallets.Wallet;

namespace HiveSpace.PaymentService.Tests.Application.Wallet;

public class GetWalletBalanceQueryHandlerTests : IClassFixture<PaymentServiceFixture>
{
    private readonly PaymentServiceFixture _fixture;

    public GetWalletBalanceQueryHandlerTests(PaymentServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithExistingWallet_ReturnsWalletDto()
    {
        var userId = Guid.NewGuid();
        var wallet = WalletAggregate.CreateForUser(userId);
        wallet.Credit(Money.FromVND(100_000), "REFUND-wallet-1", "Test credit");
        _fixture.DbContext.Wallets.Add(wallet);
        await _fixture.DbContext.SaveChangesAsync();

        var result = await BuildHandler(userId).Handle(new GetWalletQuery(userId), CancellationToken.None);

        result.UserId.Should().Be(userId);
        result.AvailableBalance.Should().Be(100_000);
    }

    [Fact]
    public async Task Handle_WhenWalletNotFound_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        var act = () => BuildHandler(userId).Handle(new GetWalletQuery(userId), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    private GetWalletQueryHandler BuildHandler(Guid userId) =>
        new(new SqlWalletRepository(_fixture.DbContext), new FakeUserContext { UserId = userId });
}
