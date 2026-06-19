using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class CouponTests
{
    public CouponTests()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public void ValidCoupon_ReducesOrderTotal()
    {
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "SAVE10",
            "Save 10",
            DiscountType.FixedAmount,
            null,
            Money.FromVND(10_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());

        var discount = coupon.CalculateDiscount(Money.FromVND(50_000));

        discount.Amount.Should().Be(10_000);
    }

    [Fact]
    public void ExpiredCoupon_IsRejected()
    {
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "OLD",
            "Expired",
            DiscountType.FixedAmount,
            null,
            Money.FromVND(10_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-2),
            DateTimeOffset.UtcNow.AddDays(-1),
            id: Guid.NewGuid());

        var result = coupon.Validate(Guid.NewGuid(), Money.FromVND(50_000));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void AlreadyUsedCoupon_IsRejected()
    {
        var userId = Guid.NewGuid();
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(),
            "ONCE",
            "Once",
            DiscountType.FixedAmount,
            null,
            Money.FromVND(10_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());
        coupon.SetMaxUsagePerUser(1);
        coupon.MarkAsUsed(userId, Guid.NewGuid(), Money.FromVND(10_000));

        var act = () => coupon.MarkAsUsed(userId, Guid.NewGuid(), Money.FromVND(10_000));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void Validate_BeforeStartDate_IsRejected()
    {
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(), "FUTURE", "Future",
            DiscountType.FixedAmount, null, Money.FromVND(10_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(2),
            id: Guid.NewGuid());

        var result = coupon.Validate(Guid.NewGuid(), Money.FromVND(50_000));

        result.IsValid.Should().BeFalse("coupon not yet started is rejected");
    }

    [Fact]
    public void Validate_WhenBelowMinOrderAmount_IsRejected()
    {
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(), "MIN100", "Min100",
            DiscountType.FixedAmount, null, Money.FromVND(10_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            minOrderAmount: Money.FromVND(100_000),
            id: Guid.NewGuid());

        var result = coupon.Validate(Guid.NewGuid(), Money.FromVND(50_000));

        result.IsValid.Should().BeFalse("order total below minimum is rejected");
    }

    [Fact]
    public void Validate_WhenMaxUsageReached_IsRejected()
    {
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(), "MAXUSE", "MaxUse",
            DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());
        coupon.SetMaxUsageCount(1);
        coupon.MarkAsUsed(Guid.NewGuid(), Guid.NewGuid(), Money.FromVND(5_000));

        var result = coupon.Validate(Guid.NewGuid(), Money.FromVND(50_000));

        result.IsValid.Should().BeFalse("coupon at max usage count is rejected");
    }

    [Fact]
    public void Validate_WhenUserExceedsMaxUsagePerUser_IsRejected()
    {
        var userId = Guid.NewGuid();
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(), "PERUSER", "PerUser",
            DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());
        coupon.SetMaxUsagePerUser(1);
        coupon.MarkAsUsed(userId, Guid.NewGuid(), Money.FromVND(5_000));

        var result = coupon.Validate(userId, Money.FromVND(50_000));

        result.IsValid.Should().BeFalse("user exceeding per-user limit is rejected");
    }

    [Fact]
    public void CalculateDiscount_FixedType_ReturnsFixedAmount()
    {
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(), "FIXED20", "Fixed20",
            DiscountType.FixedAmount, null, Money.FromVND(20_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());

        var discount = coupon.CalculateDiscount(Money.FromVND(100_000));

        discount.Amount.Should().Be(20_000L);
    }

    [Fact]
    public void CalculateDiscount_PercentageType_CappedAtMaxDiscountAmount()
    {
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(), "PCT10", "10Pct",
            DiscountType.Percentage, 10m, null,
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            maxDiscountAmount: Money.FromVND(5_000),
            id: Guid.NewGuid());

        var discount = coupon.CalculateDiscount(Money.FromVND(100_000));

        discount.Amount.Should().BeLessOrEqualTo(5_000L, "percentage discount is capped at MaxDiscountAmount");
    }

    [Fact]
    public void MarkAsUsed_IncrementsCurrentUsageCount()
    {
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(), "TRACK", "Track",
            DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());
        var initialCount = coupon.CurrentUsageCount;

        coupon.MarkAsUsed(Guid.NewGuid(), Guid.NewGuid(), Money.FromVND(5_000));

        coupon.CurrentUsageCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public void ReleaseUsage_DecrementsCurrentUsageCount()
    {
        var orderId = Guid.NewGuid();
        var coupon = Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(), "RELEASE", "Release",
            DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());
        coupon.MarkAsUsed(Guid.NewGuid(), orderId, Money.FromVND(5_000));
        var countAfterUse = coupon.CurrentUsageCount;

        coupon.ReleaseUsage(orderId);

        coupon.CurrentUsageCount.Should().Be(countAfterUse - 1);
    }

    [Fact]
    public void CreateByStore_WithFixedAmount_CreatesCoupon()
    {
        var storeId = Guid.NewGuid();

        var coupon = Coupon.CreateByStore(
            storeId, Guid.NewGuid(), "STORE10", "Store10",
            DiscountType.FixedAmount, null, Money.FromVND(10_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());

        coupon.StoreId.Should().Be(storeId);
        coupon.OwnerType.Should().Be(CouponOwnerType.Store);
        coupon.DiscountAmount!.Amount.Should().Be(10_000);
    }

    [Fact]
    public void CreateByStore_WithPercentage_CreatesCoupon()
    {
        var coupon = Coupon.CreateByStore(
            Guid.NewGuid(), Guid.NewGuid(), "PCT5", "5Pct",
            DiscountType.Percentage, 5m, null,
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());

        coupon.DiscountPercentage.Should().Be(5m);
    }

    [Fact]
    public void CreateByStore_WithInvalidFixedAmount_ThrowsDomainException()
    {
        var act = () => Coupon.CreateByStore(
            Guid.NewGuid(), Guid.NewGuid(), "C", "N",
            DiscountType.FixedAmount, null, Money.FromVND(0),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void CreateByStore_WithInvalidPercentage_ThrowsDomainException()
    {
        var act = () => Coupon.CreateByStore(
            Guid.NewGuid(), Guid.NewGuid(), "C", "N",
            DiscountType.Percentage, 150m, null,
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void CreateByStore_WithMaxDiscountTooSmall_ThrowsDomainException()
    {
        // 10% of 100_000 = 10_000 > maxDiscount 1_000 → throws
        var act = () => Coupon.CreateByStore(
            Guid.NewGuid(), Guid.NewGuid(), "C", "N",
            DiscountType.Percentage, 10m, null,
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            minOrderAmount: Money.FromVND(100_000),
            maxDiscountAmount: Money.FromVND(1_000));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void CreateByPlatform_WithInvalidFixedAmount_ThrowsDomainException()
    {
        var act = () => Coupon.CreateByPlatform(
            "admin", "C", "N", DiscountType.FixedAmount, null, Money.FromVND(0),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void CreateByPlatform_WithInvalidPercentage_ThrowsDomainException()
    {
        var act = () => Coupon.CreateByPlatform(
            "admin", "C", "N", DiscountType.Percentage, 0m, null,
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void CreateByPlatform_WithMaxDiscountTooSmall_ThrowsDomainException()
    {
        var act = () => Coupon.CreateByPlatform(
            "admin", "C", "N", DiscountType.Percentage, 10m, null,
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            minOrderAmount: Money.FromVND(100_000),
            maxDiscountAmount: Money.FromVND(500));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void SetIsHidden_SetsHiddenFlag()
    {
        var coupon = MakeOngoingCoupon();

        coupon.SetIsHidden(true);

        coupon.IsHidden.Should().BeTrue();
    }

    [Fact]
    public void LimitToProducts_SetsApplicableProductIds()
    {
        var coupon = MakeOngoingCoupon();

        coupon.LimitToProducts(new long[] { 1L, 2L, 3L });

        coupon.ApplicableProductIds.Should().BeEquivalentTo(new long[] { 1L, 2L, 3L });
    }

    [Fact]
    public void LimitToCategories_SetsApplicableCategoryIds()
    {
        var coupon = MakeOngoingCoupon();

        coupon.LimitToCategories(new int[] { 10, 20 });

        coupon.ApplicableCategoryIds.Should().BeEquivalentTo(new int[] { 10, 20 });
    }

    [Fact]
    public void AddRule_AddsCustomRule()
    {
        var coupon = MakeOngoingCoupon();

        coupon.AddRule("MIN_QTY", "qty >= 2", "Must buy at least 2");

        coupon.Rules.Should().ContainSingle(r => r.RuleName == "MIN_QTY");
    }

    [Fact]
    public void Validate_WithMatchingProductId_IsValid()
    {
        var coupon = MakeOngoingCoupon();
        coupon.LimitToProducts(new long[] { 5L });

        var result = coupon.Validate(Guid.NewGuid(), Money.FromVND(200_000), productIds: new long[] { 5L });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNoMatchingProductId_IsRejected()
    {
        var coupon = MakeOngoingCoupon();
        coupon.LimitToProducts(new long[] { 5L });

        var result = coupon.Validate(Guid.NewGuid(), Money.FromVND(200_000), productIds: new long[] { 99L });

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithNoProductIdsProvided_WhenLimited_IsRejected()
    {
        var coupon = MakeOngoingCoupon();
        coupon.LimitToProducts(new long[] { 5L });

        var result = coupon.Validate(Guid.NewGuid(), Money.FromVND(200_000));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithMatchingCategoryId_IsValid()
    {
        var coupon = MakeOngoingCoupon();
        coupon.LimitToCategories(new int[] { 10 });

        var result = coupon.Validate(Guid.NewGuid(), Money.FromVND(200_000), categoryIds: new int[] { 10 });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNoMatchingCategoryId_IsRejected()
    {
        var coupon = MakeOngoingCoupon();
        coupon.LimitToCategories(new int[] { 10 });

        var result = coupon.Validate(Guid.NewGuid(), Money.FromVND(200_000), categoryIds: new int[] { 99 });

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithNoCategoryIdsProvided_WhenLimited_IsRejected()
    {
        var coupon = MakeOngoingCoupon();
        coupon.LimitToCategories(new int[] { 10 });

        var result = coupon.Validate(Guid.NewGuid(), Money.FromVND(200_000));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithMatchingStoreId_IsValid()
    {
        var storeId = Guid.NewGuid();
        var coupon = Coupon.CreateByStore(
            storeId, Guid.NewGuid(), "S", "S", DiscountType.FixedAmount, null,
            Money.FromVND(5_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1), id: Guid.NewGuid());

        var result = coupon.Validate(Guid.NewGuid(), Money.FromVND(200_000), storeId: storeId);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNoStoreIdProvided_WhenStoreRequired_IsRejected()
    {
        var storeId = Guid.NewGuid();
        var coupon = Coupon.CreateByStore(
            storeId, Guid.NewGuid(), "S", "S", DiscountType.FixedAmount, null,
            Money.FromVND(5_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1), id: Guid.NewGuid());

        var result = coupon.Validate(Guid.NewGuid(), Money.FromVND(200_000));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithWrongStoreId_IsRejected()
    {
        var storeId = Guid.NewGuid();
        var coupon = Coupon.CreateByStore(
            storeId, Guid.NewGuid(), "S", "S", DiscountType.FixedAmount, null,
            Money.FromVND(5_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1), id: Guid.NewGuid());

        var result = coupon.Validate(Guid.NewGuid(), Money.FromVND(200_000), storeId: Guid.NewGuid());

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WhenInactive_IsRejected()
    {
        var coupon = MakeOngoingCoupon();
        coupon.Deactivate();

        var result = coupon.Validate(Guid.NewGuid(), Money.FromVND(200_000));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Activate_ActivatesCoupon()
    {
        var coupon = MakeOngoingCoupon();
        coupon.Deactivate();

        coupon.Activate();

        coupon.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_DeactivatesCoupon()
    {
        var coupon = MakeOngoingCoupon();

        coupon.Deactivate();

        coupon.IsActive.Should().BeFalse();
    }

    [Fact]
    public void End_OnActiveCoupon_SetsEndDateTimeToNow()
    {
        var coupon = MakeOngoingCoupon();

        coupon.End();

        coupon.EndDateTime.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void End_OnInactiveCoupon_ThrowsDomainException()
    {
        var coupon = MakeOngoingCoupon();
        coupon.Deactivate();

        var act = () => coupon.End();

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void UpdateName_WithValidName_UpdatesName()
    {
        var coupon = MakeOngoingCoupon();

        coupon.UpdateName("New Name");

        coupon.Name.Should().Be("New Name");
    }

    [Fact]
    public void UpdateName_WithEmptyName_ThrowsDomainException()
    {
        var coupon = MakeOngoingCoupon();

        var act = () => coupon.UpdateName("");

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void UpdateCode_WithValidCode_UppercasesAndUpdates()
    {
        var coupon = MakeOngoingCoupon();

        coupon.UpdateCode("newcode");

        coupon.Code.Should().Be("NEWCODE");
    }

    [Fact]
    public void UpdateCode_WithEmptyCode_ThrowsDomainException()
    {
        var coupon = MakeOngoingCoupon();

        var act = () => coupon.UpdateCode("");

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void UpdateMaxUsageCount_WithValidCount_UpdatesCount()
    {
        var coupon = MakeOngoingCoupon();

        coupon.UpdateMaxUsageCount(100);

        coupon.MaxUsageCount.Should().Be(100);
    }

    [Fact]
    public void UpdateMaxUsageCount_BelowCurrentUsage_ThrowsDomainException()
    {
        var coupon = MakeOngoingCoupon();
        coupon.MarkAsUsed(Guid.NewGuid(), Guid.NewGuid(), Money.FromVND(5_000));

        var act = () => coupon.UpdateMaxUsageCount(0);

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void GetRemainingUsage_WithLimit_ReturnsRemaining()
    {
        var coupon = MakeOngoingCoupon();
        coupon.SetMaxUsageCount(10);
        coupon.MarkAsUsed(Guid.NewGuid(), Guid.NewGuid(), Money.FromVND(5_000));

        coupon.GetRemainingUsage().Should().Be(9);
    }

    [Fact]
    public void GetRemainingUsage_WithNoLimit_ReturnsNull()
    {
        var coupon = MakeOngoingCoupon();

        coupon.GetRemainingUsage().Should().BeNull();
    }

    [Fact]
    public void MarkAsUsed_WithDuplicateOrderId_IsIdempotent()
    {
        var coupon = MakeOngoingCoupon();
        var orderId = Guid.NewGuid();
        coupon.MarkAsUsed(Guid.NewGuid(), orderId, Money.FromVND(5_000));
        var count = coupon.CurrentUsageCount;

        coupon.MarkAsUsed(Guid.NewGuid(), orderId, Money.FromVND(5_000));

        coupon.CurrentUsageCount.Should().Be(count);
    }

    [Fact]
    public void ReleaseUsage_WithNoMatchingOrderId_DoesNotThrow()
    {
        var coupon = MakeOngoingCoupon();

        var act = () => coupon.ReleaseUsage(Guid.NewGuid());

        act.Should().NotThrow();
    }

    [Fact]
    public void ReleaseUsage_WithEmptyOrderId_ThrowsDomainException()
    {
        var coupon = MakeOngoingCoupon();

        var act = () => coupon.ReleaseUsage(Guid.Empty);

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void MarkAsUsed_WhenCouponInvalid_ThrowsDomainException()
    {
        var coupon = MakeOngoingCoupon();
        coupon.Deactivate();

        var act = () => coupon.MarkAsUsed(Guid.NewGuid(), Guid.NewGuid(), Money.FromVND(5_000));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void MarkAsUsed_WhenMaxUsageReached_ThrowsDomainException()
    {
        var coupon = MakeOngoingCoupon();
        coupon.SetMaxUsageCount(1);
        coupon.MarkAsUsed(Guid.NewGuid(), Guid.NewGuid(), Money.FromVND(5_000));

        var act = () => coupon.MarkAsUsed(Guid.NewGuid(), Guid.NewGuid(), Money.FromVND(5_000));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void CalculateDiscount_PercentageType_WhenExceedingOrderTotal_CapsAtTotal()
    {
        var coupon = Coupon.CreateByPlatform(
            "admin", "BIG", "Big",
            DiscountType.Percentage, 100m, null,
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());

        var discount = coupon.CalculateDiscount(Money.FromVND(50_000));

        discount.Amount.Should().Be(50_000);
    }

    [Fact]
    public void Update_WhenExpired_ThrowsDomainException()
    {
        var coupon = Coupon.CreateByPlatform(
            "admin", "OLD", "Old", DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), id: Guid.NewGuid());

        var act = () => coupon.Update("Name", "CODE",
            DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(2),
            null, 10, Money.FromVND(5_000));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void Update_WhenOngoing_UpdatesNameCodeAndEnd()
    {
        var coupon = MakeOngoingCoupon();

        coupon.Update("New Name", "NEWCODE",
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow.AddDays(5),
            null, 0, Money.FromVND(5_000));

        coupon.Name.Should().Be("New Name");
        coupon.Code.Should().Be("NEWCODE");
    }

    [Fact]
    public void Update_WhenUpcoming_UpdatesAllFields()
    {
        var start = DateTimeOffset.UtcNow.AddDays(3);
        var end = DateTimeOffset.UtcNow.AddDays(6);
        var coupon = Coupon.CreateByPlatform(
            "admin", "UP", "Up", DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice, start, end, id: Guid.NewGuid());

        var newStart = DateTimeOffset.UtcNow.AddDays(4);
        var newEnd = DateTimeOffset.UtcNow.AddDays(7);
        coupon.Update("Updated", "UPDATED", newStart, newEnd, null, 50, Money.FromVND(8_000));

        coupon.Name.Should().Be("Updated");
        coupon.MaxUsageCount.Should().Be(50);
    }

    [Fact]
    public void Update_WhenUpcoming_WithPercentageType_UpdatesPercentage()
    {
        var start = DateTimeOffset.UtcNow.AddDays(3);
        var end = DateTimeOffset.UtcNow.AddDays(6);
        var coupon = Coupon.CreateByPlatform(
            "admin", "UP", "Up", DiscountType.Percentage, 10m, null,
            CouponScope.ItemPrice, start, end, id: Guid.NewGuid());

        var newStart = DateTimeOffset.UtcNow.AddDays(4);
        var newEnd = DateTimeOffset.UtcNow.AddDays(7);
        coupon.Update("Updated", "UPDATED", newStart, newEnd, null, 0, discountPercentage: 20m);

        coupon.DiscountPercentage.Should().Be(20m);
    }

    [Fact]
    public void UpdateEndDateTime_WithPastDate_ThrowsDomainException()
    {
        var coupon = MakeOngoingCoupon();

        var act = () => coupon.UpdateEndDateTime(DateTimeOffset.UtcNow.AddMinutes(-1));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void UpdateEndDateTime_WithDateBeforeStart_ThrowsDomainException()
    {
        var coupon = MakeOngoingCoupon();

        // StartDateTime is in the past, so new end must still be after it
        var act = () => coupon.UpdateEndDateTime(DateTimeOffset.UtcNow.AddMinutes(-30));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void UpdateEndDateTime_OnExpiredCoupon_ThrowsDomainException()
    {
        var coupon = Coupon.CreateByPlatform(
            "admin", "X", "X", DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), id: Guid.NewGuid());

        var act = () => coupon.UpdateEndDateTime(DateTimeOffset.UtcNow.AddDays(1));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void UpdateEarlySaveDateTime_WithSameValue_DoesNotThrow()
    {
        var coupon = MakeOngoingCoupon();

        var act = () => coupon.UpdateEarlySaveDateTime(null);

        act.Should().NotThrow();
    }

    [Fact]
    public void UpdateEarlySaveDateTime_WithValueAfterStart_ThrowsDomainException()
    {
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var coupon = Coupon.CreateByPlatform(
            "admin", "UP", "Up", DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice, start, start.AddDays(1), id: Guid.NewGuid());

        // new EarlySaveDateTime >= StartDateTime → throws
        var act = () => coupon.UpdateEarlySaveDateTime(start.AddHours(1));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void UpdateStartDateTime_WhenCouponIsOngoing_ThrowsDomainException()
    {
        var coupon = MakeOngoingCoupon();

        var act = () => coupon.UpdateStartDateTime(DateTimeOffset.UtcNow.AddDays(1));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void UpdateStartDateTime_WithPastNewStartTime_ThrowsDomainException()
    {
        var start = DateTimeOffset.UtcNow.AddDays(2);
        var coupon = Coupon.CreateByPlatform(
            "admin", "UP", "Up", DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice, start, start.AddDays(1), id: Guid.NewGuid());

        var act = () => coupon.UpdateStartDateTime(DateTimeOffset.UtcNow.AddMinutes(-1));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void UpdateStartDateTime_WithNewStartAfterEnd_ThrowsDomainException()
    {
        var start = DateTimeOffset.UtcNow.AddDays(2);
        var end = start.AddDays(1);
        var coupon = Coupon.CreateByPlatform(
            "admin", "UP", "Up", DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice, start, end, id: Guid.NewGuid());

        var act = () => coupon.UpdateStartDateTime(end.AddHours(1));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void Usages_ReturnsEmptyByDefault()
    {
        var coupon = MakeOngoingCoupon();

        coupon.Usages.Should().BeEmpty();
    }

    [Fact]
    public void CreateByStore_WithPercentageAndValidMaxDiscount_DoesNotThrow()
    {
        // minOrderAmount=100_000, percentage=10%, minExpectedDiscount=10_000, maxDiscount=15_000 >= 10_000 → OK
        var act = () => Coupon.CreateByStore(
            Guid.NewGuid(), Guid.NewGuid(), "C", "N",
            DiscountType.Percentage, 10m, null,
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            minOrderAmount: Money.FromVND(100_000),
            maxDiscountAmount: Money.FromVND(15_000));

        act.Should().NotThrow();
    }

    [Fact]
    public void SetMaxUsageCount_WithZero_ThrowsDomainException()
    {
        var coupon = MakeOngoingCoupon();

        var act = () => coupon.SetMaxUsageCount(0);

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void SetMaxUsagePerUser_WithValidCount_SetsCount()
    {
        var coupon = MakeOngoingCoupon();

        coupon.SetMaxUsagePerUser(3);

        coupon.MaxUsagePerUser.Should().Be(3);
    }

    [Fact]
    public void SetMaxUsagePerUser_WithZero_ThrowsDomainException()
    {
        var coupon = MakeOngoingCoupon();

        var act = () => coupon.SetMaxUsagePerUser(0);

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void UpdateStartDateTime_WhenEarlySaveDateConflicts_ThrowsDomainException()
    {
        // Upcoming coupon with EarlySaveDateTime set
        var start = DateTimeOffset.UtcNow.AddDays(5);
        var end = start.AddDays(2);
        var earlyDate = DateTimeOffset.UtcNow.AddDays(3);
        var coupon = Coupon.CreateByPlatform(
            "admin", "UP", "Up", DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice, start, end,
            earlySaveDateTime: earlyDate, id: Guid.NewGuid());

        // New start = AddDays(4), which is after earlyDate(3) → earlyDate >= newStart is false
        // But new start = AddDays(2), which is BEFORE earlyDate(3) → earlyDate >= newStart(2) → throws
        var newStart = DateTimeOffset.UtcNow.AddDays(2).AddHours(1);

        var act = () => coupon.UpdateStartDateTime(newStart);

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void UpdateEndDateTime_WhenNewEndBeforeStart_ThrowsDomainException()
    {
        // Upcoming coupon: start=AddDays(3), end=AddDays(5)
        // Set newEnd = AddDays(2) > now but <= StartDateTime(3) → throws
        var start = DateTimeOffset.UtcNow.AddDays(3);
        var end = DateTimeOffset.UtcNow.AddDays(5);
        var coupon = Coupon.CreateByPlatform(
            "admin", "UP", "Up", DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice, start, end, id: Guid.NewGuid());

        var act = () => coupon.UpdateEndDateTime(DateTimeOffset.UtcNow.AddDays(2));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void UpdateEarlySaveDateTime_WhenAlreadyStarted_ThrowsDomainException()
    {
        // Create upcoming coupon with earlySaveDateTime set and already in past (simulate by reflection)
        var start = DateTimeOffset.UtcNow.AddDays(5);
        var earlyDate = DateTimeOffset.UtcNow.AddDays(2);
        var coupon = Coupon.CreateByPlatform(
            "admin", "UP", "Up", DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice, start, start.AddDays(2),
            earlySaveDateTime: earlyDate, id: Guid.NewGuid());
        // Force EarlySaveDateTime to the past via reflection
        typeof(Coupon).GetProperty("EarlySaveDateTime")!
            .GetSetMethod(nonPublic: true)!
            .Invoke(coupon, new object?[] { DateTimeOffset.UtcNow.AddMinutes(-1) });

        var act = () => coupon.UpdateEarlySaveDateTime(DateTimeOffset.UtcNow.AddDays(1));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void UpdateEarlySaveDateTime_WithValidFutureDate_SetsDate()
    {
        var start = DateTimeOffset.UtcNow.AddDays(5);
        var coupon = Coupon.CreateByPlatform(
            "admin", "UP", "Up", DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice, start, start.AddDays(2), id: Guid.NewGuid());

        var newEarlyDate = DateTimeOffset.UtcNow.AddDays(2);
        coupon.UpdateEarlySaveDateTime(newEarlyDate);

        coupon.EarlySaveDateTime.Should().BeCloseTo(newEarlyDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Update_WhenUpcoming_WithNullFixedAmount_ThrowsDomainException()
    {
        var start = DateTimeOffset.UtcNow.AddDays(3);
        var end = DateTimeOffset.UtcNow.AddDays(6);
        var coupon = Coupon.CreateByPlatform(
            "admin", "UP", "Up", DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice, start, end, id: Guid.NewGuid());

        var newStart = DateTimeOffset.UtcNow.AddDays(4);
        var newEnd = DateTimeOffset.UtcNow.AddDays(7);
        var act = () => coupon.Update("Updated", "UPDATED", newStart, newEnd, null, 0, discountAmount: null);

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void Update_WhenUpcoming_WithInvalidPercentage_ThrowsDomainException()
    {
        var start = DateTimeOffset.UtcNow.AddDays(3);
        var end = DateTimeOffset.UtcNow.AddDays(6);
        var coupon = Coupon.CreateByPlatform(
            "admin", "UP", "Up", DiscountType.Percentage, 10m, null,
            CouponScope.ItemPrice, start, end, id: Guid.NewGuid());

        var newStart = DateTimeOffset.UtcNow.AddDays(4);
        var newEnd = DateTimeOffset.UtcNow.AddDays(7);
        var act = () => coupon.Update("Updated", "UPDATED", newStart, newEnd, null, 0, discountPercentage: 0m);

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    [Fact]
    public void Update_WhenUpcoming_WithMaxDiscountTooSmall_ThrowsDomainException()
    {
        var start = DateTimeOffset.UtcNow.AddDays(3);
        var end = DateTimeOffset.UtcNow.AddDays(6);
        var coupon = Coupon.CreateByPlatform(
            "admin", "UP", "Up", DiscountType.Percentage, 10m, null,
            CouponScope.ItemPrice, start, end, id: Guid.NewGuid());

        var newStart = DateTimeOffset.UtcNow.AddDays(4);
        var newEnd = DateTimeOffset.UtcNow.AddDays(7);
        // minOrderAmount=100_000, 10% of 100_000=10_000, maxDiscount=500 < 10_000 → throws
        var act = () => coupon.Update("Updated", "UPDATED", newStart, newEnd, null, 0,
            discountPercentage: 10m,
            maxDiscountAmount: Money.FromVND(500),
            minOrderAmount: Money.FromVND(100_000));

        act.Should().Throw<HiveSpace.Domain.Shared.Exceptions.DomainException>();
    }

    private static Coupon MakeOngoingCoupon() =>
        Coupon.CreateByPlatform(
            Guid.NewGuid().ToString(), "ONGOING", "Ongoing",
            DiscountType.FixedAmount, null, Money.FromVND(5_000),
            CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddDays(1),
            id: Guid.NewGuid());
}
