using FluentAssertions;
using HiveSpace.OrderService.Domain.Enumerations;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class OrderStatusTests
{
    [Fact]
    public void Created_IsFinal_ReturnsFalse()
    {
        OrderStatus.Created.IsFinal().Should().BeFalse();
    }

    [Fact]
    public void Completed_IsFinal_ReturnsTrue()
    {
        OrderStatus.Completed.IsFinal().Should().BeTrue();
    }

    [Fact]
    public void Cancelled_IsFinal_ReturnsFalse()
    {
        // Domain classifies Cancelled as terminal but not 'final' (Completed/Refunded/Solved/Expired only)
        OrderStatus.Cancelled.IsFinal().Should().BeFalse();
    }

    [Fact]
    public void Created_CanBeCancelled_ReturnsTrue()
    {
        OrderStatus.Created.CanBeCancelled().Should().BeTrue();
    }

    [Fact]
    public void Shipped_CanBeCancelled_ReturnsFalse()
    {
        OrderStatus.Shipped.CanBeCancelled().Should().BeFalse();
    }

    [Fact]
    public void Confirmed_CanBeShipped_ReturnsTrue()
    {
        OrderStatus.Confirmed.CanBeShipped().Should().BeTrue();
    }

    [Fact]
    public void Cancelled_CanBeConfirmed_ReturnsFalse()
    {
        OrderStatus.Cancelled.CanBeConfirmed().Should().BeFalse();
    }

    [Fact]
    public void Confirmed_CanBeRejected_ReturnsFalse()
    {
        OrderStatus.Confirmed.CanBeRejected().Should().BeFalse();
    }
}
