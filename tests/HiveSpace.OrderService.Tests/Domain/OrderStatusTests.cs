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

    [Fact]
    public void IsInProgress_ForCreated_ReturnsTrue()
    {
        OrderStatus.Created.IsInProgress().Should().BeTrue();
    }

    [Fact]
    public void IsInProgress_ForCompleted_ReturnsFalse()
    {
        OrderStatus.Completed.IsInProgress().Should().BeFalse();
    }

    [Fact]
    public void IsInProgress_ForCancelled_ReturnsFalse()
    {
        OrderStatus.Cancelled.IsInProgress().Should().BeFalse();
    }

    [Fact]
    public void IsInProgress_ForRejected_ReturnsFalse()
    {
        OrderStatus.Rejected.IsInProgress().Should().BeFalse();
    }

    [Fact]
    public void ReadyToShip_CanBeShipped_ReturnsTrue()
    {
        OrderStatus.ReadyToShip.CanBeShipped().Should().BeTrue();
    }

    [Fact]
    public void COD_CanBeConfirmed_ReturnsTrue()
    {
        OrderStatus.COD.CanBeConfirmed().Should().BeTrue();
    }

    [Fact]
    public void COD_CanBeRejected_ReturnsTrue()
    {
        OrderStatus.COD.CanBeRejected().Should().BeTrue();
    }

    [Fact]
    public void Paid_CanBeRejected_ReturnsTrue()
    {
        OrderStatus.Paid.CanBeRejected().Should().BeTrue();
    }

    [Fact]
    public void Refunded_IsFinal_ReturnsTrue()
    {
        OrderStatus.Refunded.IsFinal().Should().BeTrue();
    }

    [Fact]
    public void Solved_IsFinal_ReturnsTrue()
    {
        OrderStatus.Solved.IsFinal().Should().BeTrue();
    }

    [Fact]
    public void Paid_CanBeCancelled_ReturnsTrue()
    {
        OrderStatus.Paid.CanBeCancelled().Should().BeTrue();
    }

    [Fact]
    public void Confirmed_CanBeCancelled_ReturnsTrue()
    {
        OrderStatus.Confirmed.CanBeCancelled().Should().BeTrue();
    }
}
