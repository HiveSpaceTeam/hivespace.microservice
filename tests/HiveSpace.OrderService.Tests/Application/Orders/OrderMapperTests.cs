using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Orders.Mappers;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.ValueObjects;
using HiveSpace.OrderService.Tests.Domain;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.Orders;

public class OrderMapperTests
{
    public OrderMapperTests()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    private static DeliveryAddress ValidAddress() =>
        new("Test User", new PhoneNumber("0901234567"), "123 Main St", "Ward 1", "Hanoi");

    private static ProductSnapshot ValidSnapshot() =>
        ProductSnapshot.Capture(1L, 10L, "Product A", "SKU A", Money.FromVND(100_000), "img.jpg");

    [Fact]
    public void ToDetailDto_WithItemsCheckoutsAndDiscounts_MapsAllCollections()
    {
        var storeId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();
        var order = Order.Create(buyerId, ValidAddress(), storeId);
        order.AddItem(1L, 10L, 2, Money.FromVND(50_000), ValidSnapshot());

        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(), "OM_TEST01", "Mapper Coupon",
            DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow.AddDays(7));
        order.ApplyDiscount(coupon, Money.FromVND(5_000));
        order.AddCheckout(PaymentMethod.COD, Money.FromVND(95_000));

        var dto = order.ToDetailDto();

        dto.Should().NotBeNull();
        dto.UserId.Should().Be(buyerId);
        dto.StoreId.Should().Be(storeId);
        dto.Items.Should().ContainSingle();
        dto.Discounts.Should().ContainSingle();
        dto.Checkouts.Should().ContainSingle();
        dto.Trackings.Should().NotBeEmpty();
    }

    [Fact]
    public void ToBuyerSummaryDto_WithItem_MapsFieldsAndItems()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 10L, 1, Money.FromVND(100_000), ValidSnapshot());

        var dto = order.ToBuyerSummaryDto();

        dto.Should().NotBeNull();
        dto.OrderCode.Should().Be(order.OrderCode);
        dto.ItemCount.Should().Be(1);
        dto.Items.Should().ContainSingle();
        dto.TotalAmount.Should().Be(100_000);
    }

    [Fact]
    public void ToSellerSummaryDto_WithItem_MapsFieldsAndItems()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 10L, 1, Money.FromVND(100_000), ValidSnapshot());

        var dto = order.ToSellerSummaryDto();

        dto.Should().NotBeNull();
        dto.OrderCode.Should().Be(order.OrderCode);
        dto.Items.Should().ContainSingle();
    }

    [Fact]
    public void ToDetailDto_EmptyOrder_HasEmptyCollections()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());

        var dto = order.ToDetailDto();

        dto.Items.Should().BeEmpty();
        dto.Discounts.Should().BeEmpty();
        dto.Checkouts.Should().BeEmpty();
        dto.Trackings.Should().NotBeEmpty(); // "Created" tracking added on construction
    }
}
