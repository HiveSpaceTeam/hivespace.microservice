using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Orders.Queries.GetSellerOrders;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.SellerOrders;

public class GetSellerOrdersQueryHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public GetSellerOrdersQueryHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_AsSeller_ReturnsOrdersFromQuery()
    {
        var storeId = Guid.NewGuid();
        var handler = new GetSellerOrdersQueryHandler(
            new FakeOrderDataQuery(),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Seller"], StoreId = storeId });

        var result = await handler.Handle(
            new GetSellerOrdersQuery(1, 20),
            CancellationToken.None);

        result.Should().NotBeNull();
        result.Orders.Should().BeEmpty();
        result.Pagination.CurrentPage.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNoStoreId_ThrowsForbiddenException()
    {
        var handler = new GetSellerOrdersQueryHandler(
            new FakeOrderDataQuery(),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Seller"] });

        var act = () => handler.Handle(new GetSellerOrdersQuery(1, 20), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
