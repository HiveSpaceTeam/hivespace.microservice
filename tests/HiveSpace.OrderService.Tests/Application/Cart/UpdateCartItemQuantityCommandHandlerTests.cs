using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.OrderService.Application.Cart.Commands.UpdateCartItems;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CartAggregate = HiveSpace.OrderService.Domain.Aggregates.Carts.Cart;

namespace HiveSpace.OrderService.Tests.Application.Cart;

public class UpdateCartItemQuantityCommandHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public UpdateCartItemQuantityCommandHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_WithValidUpdate_ChangesItemQuantity()
    {
        var userId = Guid.NewGuid();
        var cart = CartAggregate.Create(userId);
        cart.AddItem(1L, 10L, 2);
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var cartItemId = cart.Items.First().Id;

        var handler = new UpdateCartItemsCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlProductRefRepository(_fixture.DbContext),
            new SqlSkuRefRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var command = new UpdateCartItemsCommand
        {
            Items = [new CartItemUpdateRequest(cartItemId, null, 5, null)]
        };

        await handler.Handle(command, CancellationToken.None);

        var stored = await _fixture.DbContext.Carts
            .Include(c => c.Items)
            .SingleAsync(c => c.UserId == userId);
        stored.Items.Single().Quantity.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WithNoCart_ThrowsNotFoundException()
    {
        var handler = new UpdateCartItemsCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlProductRefRepository(_fixture.DbContext),
            new SqlSkuRefRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid() });

        var command = new UpdateCartItemsCommand
        {
            Items = [new CartItemUpdateRequest(Guid.NewGuid(), null, 3, null)]
        };

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithSelectAll_SelectsAllCartItems()
    {
        var userId = Guid.NewGuid();
        var cart = CartAggregate.Create(userId);
        cart.AddItem(1L, 10L, 2);
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateCartItemsCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlProductRefRepository(_fixture.DbContext),
            new SqlSkuRefRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        await handler.Handle(
            new UpdateCartItemsCommand { SelectAll = true, Items = [] },
            CancellationToken.None);

        var stored = await _fixture.DbContext.Carts
            .Include(c => c.Items)
            .SingleAsync(c => c.UserId == userId);
        stored.Items.Should().AllSatisfy(i => i.IsSelected.Should().BeTrue());
    }

    [Fact]
    public async Task Handle_WithAppliedStoreCoupon_ExecutesCouponCleanupPath()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var cart = CartAggregate.Create(userId);
        cart.AddItem(1L, 10L, 2);
        cart.ApplyStoreCoupon(storeId, "UCQH_CLEANUP1");
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateCartItemsCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlProductRefRepository(_fixture.DbContext),
            new SqlSkuRefRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        var act = () => handler.Handle(new UpdateCartItemsCommand { Items = [] }, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithInvalidSkuForProduct_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        var cart = CartAggregate.Create(userId);
        cart.AddItem(1L, 10L, 2);
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var cartItemId = cart.Items.First().Id;

        var handler = new UpdateCartItemsCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlProductRefRepository(_fixture.DbContext),
            new SqlSkuRefRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        // SkuId 99999 does not exist in SkuRefs → ExistsAsync returns false → NotFoundException
        var act = () => handler.Handle(
            new UpdateCartItemsCommand
            {
                Items = [new CartItemUpdateRequest(cartItemId, 99999L, null, null)]
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithSkuUpdateForMissingCartItem_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        var cart = CartAggregate.Create(userId);
        cart.AddItem(1L, 10L, 2);
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var handler = new UpdateCartItemsCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlProductRefRepository(_fixture.DbContext),
            new SqlSkuRefRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        // CartItemId doesn't exist in cart.Items → FirstOrDefault returns null → NotFoundException
        var act = () => handler.Handle(
            new UpdateCartItemsCommand
            {
                Items = [new CartItemUpdateRequest(Guid.NewGuid(), 10L, null, null)]
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithValidSkuId_UpdatesItemSkuSuccessfully()
    {
        var userId = Guid.NewGuid();
        var skuRef = new HiveSpace.OrderService.Domain.External.SkuRef(20L, 1L, "SKU-20", 60_000, "VND", null, null);
        _fixture.DbContext.SkuRefs.Add(skuRef);

        var cart = CartAggregate.Create(userId);
        cart.AddItem(1L, 10L, 2);
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var cartItemId = cart.Items.First().Id;

        var handler = new UpdateCartItemsCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlProductRefRepository(_fixture.DbContext),
            new SqlSkuRefRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        // SkuId=20 exists in SkuRefs with productId=1 → skuExists=true → no throw → UpdateItemById called
        var act = () => handler.Handle(
            new UpdateCartItemsCommand
            {
                Items = [new CartItemUpdateRequest(cartItemId, 20L, null, null)]
            },
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_WithStoreCouponAndDeselectedItem_ExecutesCleanupWithEmptyProductMap()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var cart = CartAggregate.Create(userId);
        cart.AddItem(1L, 10L, 2);  // IsSelected = true by default
        cart.ApplyStoreCoupon(storeId, "UCQH_DESEL1");
        _fixture.DbContext.Carts.Add(cart);
        await _fixture.DbContext.SaveChangesAsync();

        var cartItemId = cart.Items.First().Id;

        var handler = new UpdateCartItemsCommandHandler(
            new SqlCartRepository(_fixture.DbContext),
            new SqlCouponRepository(_fixture.DbContext),
            new SqlProductRefRepository(_fixture.DbContext),
            new SqlSkuRefRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId });

        // Deselect item → selectedItems=[] → productIds.Count=0 and skuIds.Count=0 branches
        var act = () => handler.Handle(
            new UpdateCartItemsCommand
            {
                Items = [new CartItemUpdateRequest(cartItemId, null, null, false)]
            },
            CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
