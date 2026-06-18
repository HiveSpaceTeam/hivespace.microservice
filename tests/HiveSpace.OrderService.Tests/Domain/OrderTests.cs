using FluentAssertions;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
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

    [Fact]
    public void Create_WithEmptyUserId_ThrowsDomainException()
    {
        var act = () => Order.Create(Guid.Empty, ValidAddress(), Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithNullAddress_ThrowsDomainException()
    {
        var act = () => Order.Create(Guid.NewGuid(), null!, Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithEmptyStoreId_ThrowsDomainException()
    {
        var act = () => Order.Create(Guid.NewGuid(), ValidAddress(), Guid.Empty);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithExplicitId_UsesProvidedId()
    {
        var id = Guid.NewGuid();

        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid(), id);

        order.Id.Should().Be(id);
    }

    [Fact]
    public void SetShippingFee_WithValidFee_UpdatesTotalAmount()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(100_000), ValidSnapshot());

        order.SetShippingFee(Money.FromVND(20_000), isShippingPaidBySeller: false);

        order.ShippingFee.Amount.Should().Be(20_000);
        order.TotalAmount.Amount.Should().Be(120_000);
    }

    [Fact]
    public void SetShippingFee_WithSellerPaidShipping_DoesNotAddToBuyerTotal()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(100_000), ValidSnapshot());

        order.SetShippingFee(Money.FromVND(20_000), isShippingPaidBySeller: true);

        order.TotalAmount.Amount.Should().Be(100_000);
        order.IsShippingPaidBySeller.Should().BeTrue();
    }

    [Fact]
    public void SetShippingFee_WithNullFee_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());

        var act = () => order.SetShippingFee(null!, false);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddItem_WhenDiscountAlreadyApplied_ThrowsDomainException()
    {
        var storeId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), storeId);
        order.AddItem(1L, 1L, 1, Money.FromVND(100_000), ValidSnapshot());
        var coupon = MakeStoreCoupon(storeId);
        order.ApplyDiscount(coupon, Money.FromVND(10_000));

        var act = () => order.AddItem(2L, 2L, 1, Money.FromVND(50_000), ValidSnapshot());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void SetShippingFee_WhenDiscountAlreadyApplied_ThrowsDomainException()
    {
        var storeId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), storeId);
        order.AddItem(1L, 1L, 1, Money.FromVND(100_000), ValidSnapshot());
        var coupon = MakeStoreCoupon(storeId);
        order.ApplyDiscount(coupon, Money.FromVND(10_000));

        var act = () => order.SetShippingFee(Money.FromVND(20_000), false);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddCheckout_AddsCheckoutRecord()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());

        order.AddCheckout(PaymentMethod.VNPAY, Money.FromVND(50_000));

        order.Checkouts.Should().ContainSingle();
        order.Checkouts.Single().Amount.Amount.Should().Be(50_000);
    }

    [Fact]
    public void ApplyDiscount_WithPlatformCoupon_AppliesDiscount()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(100_000), ValidSnapshot());
        var coupon = Coupon.CreateByPlatform(
            "admin", "PLAT10", "Platform10", DiscountType.FixedAmount,
            null, Money.FromVND(10_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1), id: Guid.NewGuid());

        order.ApplyDiscount(coupon);

        order.Discounts.Should().ContainSingle();
        order.TotalAmount.Amount.Should().BeLessThan(100_000);
    }

    [Fact]
    public void ApplyDiscount_WithNullCoupon_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());

        var act = () => order.ApplyDiscount(null!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ApplyDiscount_WithStoreCouponForWrongStore_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(100_000), ValidSnapshot());
        var wrongStoreCoupon = MakeStoreCoupon(Guid.NewGuid());

        var act = () => order.ApplyDiscount(wrongStoreCoupon);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ApplyDiscount_WithExplicitAmount_AppliesProvidedAmount()
    {
        var storeId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), storeId);
        order.AddItem(1L, 1L, 1, Money.FromVND(100_000), ValidSnapshot());
        var coupon = MakeStoreCoupon(storeId);

        order.ApplyDiscount(coupon, Money.FromVND(15_000));

        order.TotalDiscount.Amount.Should().Be(15_000);
    }

    [Fact]
    public void ApplyDiscount_WithExplicitAmount_NullCoupon_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());

        var act = () => order.ApplyDiscount(null!, Money.FromVND(10_000));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ApplyDiscount_WithExplicitAmount_WrongStore_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(100_000), ValidSnapshot());
        var wrongStoreCoupon = MakeStoreCoupon(Guid.NewGuid());

        var act = () => order.ApplyDiscount(wrongStoreCoupon, Money.FromVND(10_000));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ApplyDiscount_InNonCreatedStatus_ThrowsDomainException()
    {
        var storeId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), storeId);
        order.MarkAsExpired();

        var act = () => order.ApplyDiscount(MakeStoreCoupon(storeId), Money.FromVND(10_000));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ApplyProratedDiscount_WithPlatformCoupon_AppliesDiscount()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(100_000), ValidSnapshot());
        var coupon = Coupon.CreateByPlatform(
            "admin", "PLAT5", "Plat5", DiscountType.FixedAmount,
            null, Money.FromVND(5_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1), id: Guid.NewGuid());

        order.ApplyProratedDiscount(coupon, Money.FromVND(5_000));

        order.Discounts.Should().ContainSingle();
    }

    [Fact]
    public void ApplyProratedDiscount_WithStoreCoupon_ThrowsDomainException()
    {
        var storeId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), storeId);
        order.AddItem(1L, 1L, 1, Money.FromVND(100_000), ValidSnapshot());

        var act = () => order.ApplyProratedDiscount(MakeStoreCoupon(storeId), Money.FromVND(5_000));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ApplyProratedDiscount_WithZeroAmount_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        var coupon = Coupon.CreateByPlatform(
            "admin", "PLAT5", "Plat5", DiscountType.FixedAmount,
            null, Money.FromVND(5_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1), id: Guid.NewGuid());

        var act = () => order.ApplyProratedDiscount(coupon, Money.FromVND(0));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ApplyProratedDiscount_WithNullCoupon_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());

        var act = () => order.ApplyProratedDiscount(null!, Money.FromVND(5_000));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ApplyProratedDiscount_InNonCreatedStatus_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.MarkAsExpired();
        var coupon = Coupon.CreateByPlatform(
            "admin", "P", "P", DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1), id: Guid.NewGuid());

        var act = () => order.ApplyProratedDiscount(coupon, Money.FromVND(5_000));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsPaid_WhenAlreadyPaid_IsIdempotent()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.MarkAsPaid(Guid.NewGuid());

        var act = () => order.MarkAsPaid(Guid.NewGuid());

        act.Should().NotThrow();
        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public void MarkAsCOD_FromCreatedStatus_TransitionsToCOD()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(100_000), ValidSnapshot(), isCOD: true);

        order.MarkAsCOD();

        order.Status.Should().Be(OrderStatus.COD);
    }

    [Fact]
    public void MarkAsCOD_FromPaidStatus_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.MarkAsPaid(Guid.NewGuid());

        var act = () => order.MarkAsCOD();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Confirm_WithEmptyExecutorId_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(50_000), ValidSnapshot());
        order.MarkAsPaid(Guid.NewGuid());

        var act = () => order.Confirm(Guid.Empty);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Confirm_FromCreatedStatus_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(50_000), ValidSnapshot());

        var act = () => order.Confirm(Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reject_FromPaidStatus_Transitions()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(50_000), ValidSnapshot());
        order.MarkAsPaid(Guid.NewGuid());

        order.Reject("Out of stock", Guid.NewGuid());

        order.Status.Should().Be(OrderStatus.Rejected);
        order.RejectionReason.Should().Be("Out of stock");
    }

    [Fact]
    public void Reject_WithEmptyExecutorId_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.MarkAsPaid(Guid.NewGuid());

        var act = () => order.Reject("reason", Guid.Empty);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reject_WithEmptyReason_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.MarkAsPaid(Guid.NewGuid());

        var act = () => order.Reject("", Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reject_FromConfirmedStatus_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(50_000), ValidSnapshot());
        order.MarkAsPaid(Guid.NewGuid());
        order.Confirm(Guid.NewGuid());

        var act = () => order.Reject("reason", Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AssignShipping_FromConfirmedStatus_AssignsShipping()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(50_000), ValidSnapshot());
        order.MarkAsPaid(Guid.NewGuid());
        order.Confirm(Guid.NewGuid());
        var shippingId = Guid.NewGuid();

        order.AssignShipping(shippingId);

        order.ShippingId.Should().Be(shippingId);
        order.Status.Should().Be(OrderStatus.ReadyToShip);
    }

    [Fact]
    public void AssignShipping_WithEmptyId_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(50_000), ValidSnapshot());
        order.MarkAsPaid(Guid.NewGuid());
        order.Confirm(Guid.NewGuid());

        var act = () => order.AssignShipping(Guid.Empty);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AssignShipping_FromNonConfirmedStatus_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());

        var act = () => order.AssignShipping(Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Ship_FromReadyToShipStatus_TransitionsToShipped()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(50_000), ValidSnapshot());
        order.MarkAsPaid(Guid.NewGuid());
        order.Confirm(Guid.NewGuid());
        order.AssignShipping(Guid.NewGuid());

        order.Ship();

        order.Status.Should().Be(OrderStatus.Shipped);
    }

    [Fact]
    public void Ship_FromInvalidStatus_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());

        var act = () => order.Ship();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Ship_WithNoShippingId_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(50_000), ValidSnapshot());
        order.MarkAsPaid(Guid.NewGuid());
        order.Confirm(Guid.NewGuid());

        // Confirm puts it into Confirmed status — CanBeShipped() is true but ShippingId is null
        var act = () => order.Ship();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsDelivered_FromShippedStatus_TransitionsToDelivered()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(50_000), ValidSnapshot());
        order.MarkAsPaid(Guid.NewGuid());
        order.Confirm(Guid.NewGuid());
        order.AssignShipping(Guid.NewGuid());
        order.Ship();

        order.MarkAsDelivered();

        order.Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void MarkAsDelivered_FromNonShippedStatus_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());

        var act = () => order.MarkAsDelivered();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Complete_FromDeliveredStatus_TransitionsToCompleted()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(50_000), ValidSnapshot());
        order.MarkAsPaid(Guid.NewGuid());
        order.Confirm(Guid.NewGuid());
        order.AssignShipping(Guid.NewGuid());
        order.Ship();
        order.MarkAsDelivered();

        order.Complete();

        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public void Complete_FromNonDeliveredStatus_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());

        var act = () => order.Complete();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_FromCreatedStatus_TransitionsToCancelled()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());

        order.Cancel("Changed mind", Guid.NewGuid());

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void MarkAsExpired_FromNonCreatedStatus_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.MarkAsExpired();

        var act = () => order.MarkAsExpired();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void GetCODAmount_WithCODItems_ReturnsCODTotal()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 2, Money.FromVND(50_000), ValidSnapshot(), isCOD: true);
        order.AddItem(2L, 2L, 1, Money.FromVND(30_000), ValidSnapshot(), isCOD: false);

        var codAmount = order.GetCODAmount();

        codAmount.Amount.Should().Be(100_000);
    }

    [Fact]
    public void HasCODItems_WithCODItems_ReturnsTrue()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(50_000), ValidSnapshot(), isCOD: true);

        order.HasCODItems().Should().BeTrue();
    }

    [Fact]
    public void HasCODItems_WithNoCODItems_ReturnsFalse()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(50_000), ValidSnapshot(), isCOD: false);

        order.HasCODItems().Should().BeFalse();
    }

    [Fact]
    public void CalculateSellerPayout_WithShippingPaidBySeller_DeductsShipping()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(100_000), ValidSnapshot());
        order.SetShippingFee(Money.FromVND(10_000), isShippingPaidBySeller: true);

        var payout = order.CalculateSellerPayout();

        // 100_000 - 9_900 service fee - 10_000 shipping = 80_100
        payout.Amount.Should().Be(80_100);
    }

    [Fact]
    public void CalculateSellerPayout_WithStoreDiscount_DeductsFromBase()
    {
        var storeId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), storeId);
        order.AddItem(1L, 1L, 1, Money.FromVND(100_000), ValidSnapshot());
        var coupon = MakeStoreCoupon(storeId);
        order.ApplyDiscount(coupon, Money.FromVND(20_000));

        var payout = order.CalculateSellerPayout();

        // base = 100_000 - 20_000 = 80_000; fee = 80_000 * 0.099 = 7920; payout = 80_000 - 7920 = 72_080
        payout.Amount.Should().Be(72_080);
    }

    [Fact]
    public void ApplyDiscount_WhenStatusNotCreated_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.MarkAsPaid(Guid.NewGuid());
        var coupon = Coupon.CreateByPlatform(
            "admin", "SAVE", "Save", DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1), id: Guid.NewGuid());

        var act = () => order.ApplyDiscount(coupon);

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void MarkAsCOD_WhenAmountExceedsLimit_ThrowsDomainException()
    {
        // COD limit is 2,000,000 VND
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.AddItem(1L, 1L, 1, Money.FromVND(2_100_000), ValidSnapshot(), isCOD: true);

        var act = () => order.MarkAsCOD();

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    private static Coupon MakeStoreCoupon(Guid storeId) =>
        Coupon.CreateByStore(
            storeId, Guid.NewGuid(), "STORE5", "Store5",
            DiscountType.FixedAmount,
            null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());
}
