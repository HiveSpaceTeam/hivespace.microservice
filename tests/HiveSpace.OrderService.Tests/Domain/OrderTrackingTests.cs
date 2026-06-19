using FluentAssertions;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Enumerations;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class OrderTrackingTests
{
    public OrderTrackingTests()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public void Create_WithExecutorId_StoresAllFields()
    {
        var executorId = Guid.NewGuid();

        var tracking = OrderTracking.Create(OrderTrackingType.Confirmed, ExecutorType.User, executorId, "Order confirmed");

        tracking.Type.Should().Be(OrderTrackingType.Confirmed);
        tracking.ExecutorType.Should().Be(ExecutorType.User);
        tracking.ExecutorId.Should().Be(executorId);
        tracking.Message.Should().Be("Order confirmed");
        tracking.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithNullExecutorId_StoresNullExecutorId()
    {
        var tracking = OrderTracking.Create(OrderTrackingType.Paid, ExecutorType.System, null, "System action");

        tracking.ExecutorId.Should().BeNull();
        tracking.ExecutorType.Should().Be(ExecutorType.System);
    }

    [Fact]
    public void Create_AssignsUniqueId()
    {
        var t1 = OrderTracking.Create(OrderTrackingType.Created, ExecutorType.System, null, "a");
        var t2 = OrderTracking.Create(OrderTrackingType.Created, ExecutorType.System, null, "b");

        t1.Id.Should().NotBe(t2.Id);
    }
}
