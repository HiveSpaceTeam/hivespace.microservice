using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.ValueObjects;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class OrderItemTests
{
    public OrderItemTests()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    private static ProductSnapshot ValidSnapshot() =>
        ProductSnapshot.Capture(1L, 1L, "Product A", "SKU A", Money.FromVND(100_000), "img.jpg");

    [Fact]
    public void Create_WithValidFields_CalculatesLineTotal()
    {
        var item = OrderItem.Create(1L, 1L, 3, Money.FromVND(50_000), ValidSnapshot());

        item.Quantity.Should().Be(3);
        item.UnitPrice.Amount.Should().Be(50_000);
        item.LineTotal.Amount.Should().Be(150_000);
    }

    [Fact]
    public void Create_WithIsCODTrue_SetsFlag()
    {
        var item = OrderItem.Create(1L, 1L, 1, Money.FromVND(50_000), ValidSnapshot(), isCOD: true);

        item.IsCOD.Should().BeTrue();
    }

    [Fact]
    public void Create_WithIsCODFalse_DefaultsFalse()
    {
        var item = OrderItem.Create(1L, 1L, 1, Money.FromVND(50_000), ValidSnapshot());

        item.IsCOD.Should().BeFalse();
    }

    [Fact]
    public void Create_WithZeroQuantity_ThrowsDomainException()
    {
        var act = () => OrderItem.Create(1L, 1L, 0, Money.FromVND(50_000), ValidSnapshot());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithNegativeQuantity_ThrowsDomainException()
    {
        var act = () => OrderItem.Create(1L, 1L, -1, Money.FromVND(50_000), ValidSnapshot());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithZeroUnitPrice_ThrowsDomainException()
    {
        var act = () => OrderItem.Create(1L, 1L, 1, Money.FromVND(0), ValidSnapshot());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithNullUnitPrice_ThrowsDomainException()
    {
        var act = () => OrderItem.Create(1L, 1L, 1, null!, ValidSnapshot());

        act.Should().Throw<DomainException>();
    }
}
