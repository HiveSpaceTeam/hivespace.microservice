using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.Exceptions;
using Xunit;

namespace HiveSpace.OrderService.Tests.Domain;

public class CouponEntityTests
{
    public CouponEntityTests()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    // ── CouponRule ────────────────────────────────────────────────────────────

    [Fact]
    public void CouponRule_Create_WithValidFields_StoresAllFields()
    {
        var rule = CouponRule.Create("MIN_ORDER", "total >= 100", "Order must be at least 100");

        rule.RuleName.Should().Be("MIN_ORDER");
        rule.RuleExpression.Should().Be("total >= 100");
        rule.ErrorMessage.Should().Be("Order must be at least 100");
        rule.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CouponRule_Create_AssignsUniqueId()
    {
        var r1 = CouponRule.Create("R1", "expr1", "err1");
        var r2 = CouponRule.Create("R2", "expr2", "err2");

        r1.Id.Should().NotBe(r2.Id);
    }

    // ── CouponUsage ───────────────────────────────────────────────────────────

    [Fact]
    public void CouponUsage_Create_WithValidFields_StoresAllFields()
    {
        var couponId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var discount = Money.FromVND(10_000);

        var usage = CouponUsage.Create(couponId, userId, orderId, discount);

        usage.CouponId.Should().Be(couponId);
        usage.UserId.Should().Be(userId);
        usage.OrderId.Should().Be(orderId);
        usage.DiscountAmount.Amount.Should().Be(10_000);
        usage.UsedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CouponUsage_Create_AssignsUniqueId()
    {
        var u1 = CouponUsage.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Money.FromVND(1));
        var u2 = CouponUsage.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Money.FromVND(1));

        u1.Id.Should().NotBe(u2.Id);
    }

    // ── CouponValidationResult ────────────────────────────────────────────────

    [Fact]
    public void CouponValidationResult_Valid_IsValid()
    {
        var result = CouponValidationResult.Valid();

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void CouponValidationResult_Invalid_ContainsErrors()
    {
        var error = new CouponValidationError(OrderDomainErrorCode.CouponExpired, "EndDateTime");

        var result = CouponValidationResult.Invalid(error);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.ErrorCode == OrderDomainErrorCode.CouponExpired);
    }

    [Fact]
    public void CouponValidationResult_Constructor_WithErrors_IsNotValid()
    {
        var errors = new[] { new CouponValidationError(OrderDomainErrorCode.CouponNotActive, "IsActive") };

        var result = new CouponValidationResult(false, errors);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void CouponValidationResult_Constructor_WithNoErrors_IsValid()
    {
        var result = new CouponValidationResult(true);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
