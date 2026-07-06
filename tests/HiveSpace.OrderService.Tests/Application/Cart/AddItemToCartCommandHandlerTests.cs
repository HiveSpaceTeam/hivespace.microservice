using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Cart.Commands.AddCartItem;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CartAggregate = HiveSpace.OrderService.Domain.Aggregates.Carts.Cart;

namespace HiveSpace.OrderService.Tests.Application.Cart;

public class AddItemToCartCommandHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public AddItemToCartCommandHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithValidSku_CreatesCartAndReturnsItemId()
    {
        await SeedCurrencyPolicyAsync("VND");

        var sku = new HiveSpace.OrderService.Domain.External.SkuRef(
            10L, 1L, "SKU-01", 50_000, "VND", null, null);
        _fixture.DbContext.SkuRefs.Add(sku);
        await _fixture.DbContext.SaveChangesAsync();

        var userId = Guid.NewGuid();
        var handler = new AddCartItemCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new SqlSkuRefRepository(_fixture.DbContext),
            new SqlPlatformCurrencyPolicyRefRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var cartItemId = await handler.Handle(
            new AddCartItemCommand(1L, 10L, 2), CancellationToken.None);

        cartItemId.Should().NotBeEmpty();
        var cart = await _fixture.DbContext.Carts
            .Include(c => c.Items)
            .SingleAsync(c => c.UserId == userId);
        cart.Items.Should().ContainSingle(i => i.ProductId == 1L && i.Quantity == 2);
    }

    [Fact]
    public async Task Handle_WithNonExistentSku_ThrowsNotFoundException()
    {
        await SeedCurrencyPolicyAsync("VND");

        var userId = Guid.NewGuid();
        var handler = new AddCartItemCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new SqlSkuRefRepository(_fixture.DbContext),
            new SqlPlatformCurrencyPolicyRefRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var act = () => handler.Handle(
            new AddCartItemCommand(999L, 9999L, 1), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithDisabledSkuCurrency_ThrowsInvalidFieldException()
    {
        await SeedCurrencyPolicyAsync("VND");

        _fixture.DbContext.SkuRefs.Add(new SkuRef(11L, 2L, "SKU-USD", 2_505L, "USD", null, null));
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new AddCartItemCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new SqlSkuRefRepository(_fixture.DbContext),
            new SqlPlatformCurrencyPolicyRefRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid() });

        var act = () => handler.Handle(new AddCartItemCommand(2L, 11L, 1), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidFieldException>()
            .Where(x => x.ErrorCode == HiveSpace.OrderService.Domain.Exceptions.OrderDomainErrorCode.PlatformCurrencyDisabled);
    }

    [Fact]
    public async Task Handle_WhenCartWouldBecomeMixedCurrency_ThrowsInvalidFieldException()
    {
        await SeedCurrencyPolicyAsync("VND", "USD");

        var userId = Guid.NewGuid();
        _fixture.DbContext.SkuRefs.AddRange(
            new SkuRef(12L, 3L, "SKU-VND", 50_000L, "VND", null, null),
            new SkuRef(13L, 4L, "SKU-USD", 2_505L, "USD", null, null));
        var cart = CartAggregate.Create(userId);
        cart.AddItem(3L, 12L, 1);
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new AddCartItemCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new SqlSkuRefRepository(_fixture.DbContext),
            new SqlPlatformCurrencyPolicyRefRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var act = () => handler.Handle(new AddCartItemCommand(4L, 13L, 1), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidFieldException>()
            .Where(x => x.ErrorCode == HiveSpace.OrderService.Domain.Exceptions.OrderDomainErrorCode.CheckoutMixedCurrencyNotAllowed);
    }

    private async Task SeedCurrencyPolicyAsync(params string[] enabledCurrencyCodes)
    {
        var policy = await _fixture.DbContext.PlatformCurrencyPolicyRefs.SingleOrDefaultAsync();
        if (policy is null)
        {
            _fixture.DbContext.PlatformCurrencyPolicyRefs.Add(
                new PlatformCurrencyPolicyRef(
                    Guid.NewGuid(),
                    enabledCurrencyCodes[0],
                    1,
                    DateTimeOffset.UtcNow,
                    enabledCurrencyCodes));
        }
        else
        {
            policy.Update(enabledCurrencyCodes[0], policy.Version + 1, DateTimeOffset.UtcNow, enabledCurrencyCodes);
        }

        await _fixture.DbContext.SaveChangesAsync();
    }
}
