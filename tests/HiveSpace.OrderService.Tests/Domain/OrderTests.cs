using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.ValueObjects;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class OrderTests
{
    public OrderTests()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    private static DeliveryAddress ValidAddress() =>
        new("Test User", new PhoneNumber("0901234567"), "123 Main St", "Ward 1", "Hanoi");

    private static ProductSnapshot ValidSnapshot() =>
        ProductSnapshot.Capture(1L, 1L, "Product A", "SKU A", Money.FromVND(100_000), "img.jpg");

    [Fact]
    public void Create_WithValidFields_StartsInCreatedStatus()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.Status.Should().Be(OrderStatus.Created);
    }

    [Fact]
    public void AddItem_InCreatedStatus_AddsToItemCollection()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 2, Money.FromVND(50_000), ValidSnapshot());
        order.Items.Should().ContainSingle();
    }

    [Fact]
    public void AddItem_InConfirmedStatus_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(50_000), ValidSnapshot());
        order.MarkAsPaid(Guid.NewGuid());
        order.Confirm(Guid.NewGuid());

        var act = () => order.AddItem(2L, 2L, 1, Money.FromVND(50_000), ValidSnapshot());
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsPaid_FromCreatedStatus_TransitionsToPaid()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.MarkAsPaid(Guid.NewGuid());
        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public void MarkAsPaid_FromExpiredStatus_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.MarkAsExpired();

        var act = () => order.MarkAsPaid(Guid.NewGuid());
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Confirm_WithNoItems_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.MarkAsPaid(Guid.NewGuid());

        var act = () => order.Confirm(Guid.NewGuid());
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_AfterDelivered_ThrowsDomainException()
    {
        // Expired is the simplest non-cancellable terminal state from Created
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.MarkAsExpired();

        var act = () => order.Cancel("reason", Guid.NewGuid());
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RecalculateTotals_ReflectsItemSum()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 2, Money.FromVND(100_000), ValidSnapshot());

        order.SubTotal.Amount.Should().Be(200_000);
        order.TotalAmount.Amount.Should().Be(200_000);
    }

    [Fact]
    public void CalculateSellerPayout_Applies9Point9PercentServiceFee()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(100_000), ValidSnapshot());

        var payout = order.CalculateSellerPayout();

        // 100_000 * (1 - 0.099) = 100_000 - 9_900 = 90_100
        payout.Amount.Should().Be(90_100);
    }
}
