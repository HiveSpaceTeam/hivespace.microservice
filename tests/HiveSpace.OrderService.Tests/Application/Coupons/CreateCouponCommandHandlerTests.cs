using FluentAssertions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.OrderService.Application.Coupons.Commands.CreateCoupon;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Infrastructure.Repositories;
using HiveSpace.OrderService.Tests.Domain;
using HiveSpace.OrderService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.OrderService.Tests.Application.Coupons;

public class CreateCouponCommandHandlerTests : IClassFixture<OrderServiceFixture>
{
    private readonly OrderServiceFixture _fixture;

    public CreateCouponCommandHandlerTests(OrderServiceFixture fixture)
    {
        _fixture = fixture;
        OrderIdGeneratorFixture.EnsureInitialized();
    }

    [Fact]
    public async Task Handle_AsPlatformUser_PersistsPlatformCoupon()
    {
        var userId = Guid.NewGuid();
        var handler = new CreateCouponCommandHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = userId, Roles = ["Admin"] });

        var command = new CreateCouponCommand
        {
            Code            = "PLAT_NEW",
            Name            = "Platform New",
            DiscountType    = DiscountType.FixedAmount,
            DiscountAmount  = 10_000,
            DiscountCurrency= "VND",
            Scope           = CouponScope.ItemPrice,
            StartDateTime   = DateTimeOffset.UtcNow.AddMinutes(-1),
            EndDateTime     = DateTimeOffset.UtcNow.AddDays(7)
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be("PLAT_NEW");

        var stored = await _fixture.DbContext.Coupons.SingleAsync(c => c.Code == "PLAT_NEW");
        stored.Name.Should().Be("Platform New");
    }

    [Fact]
    public async Task Handle_AsSellerUser_PersistsStoreCoupon()
    {
        var storeId = Guid.NewGuid();
        var handler = new CreateCouponCommandHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Seller"], StoreId = storeId });

        var command = new CreateCouponCommand
        {
            Code            = "STORE_NEW",
            Name            = "Store New",
            DiscountType    = DiscountType.FixedAmount,
            DiscountAmount  = 5_000,
            DiscountCurrency= "VND",
            Scope           = CouponScope.ItemPrice,
            StartDateTime   = DateTimeOffset.UtcNow.AddMinutes(-1),
            EndDateTime     = DateTimeOffset.UtcNow.AddDays(7)
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        var stored = await _fixture.DbContext.Coupons.SingleAsync(c => c.Code == "STORE_NEW");
        stored.StoreId.Should().Be(storeId);
    }

    [Fact]
    public async Task Handle_WithAllOptionalLimits_SetsUsageAndProductConstraints()
    {
        var handler = new CreateCouponCommandHandler(
            new SqlCouponRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid(), Roles = ["Admin"] });

        var command = new CreateCouponCommand
        {
            Code                 = "LIMITS01",
            Name                 = "Limited Coupon",
            DiscountType         = DiscountType.FixedAmount,
            DiscountAmount       = 5_000,
            DiscountCurrency     = "VND",
            Scope                = CouponScope.ItemPrice,
            StartDateTime        = DateTimeOffset.UtcNow.AddDays(-1),
            EndDateTime          = DateTimeOffset.UtcNow.AddDays(7),
            MaxUsageCount        = 100,
            MaxUsagePerUser      = 1,
            ApplicableProductIds  = [1L, 2L],
            ApplicableCategoryIds = [10, 20]
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be("LIMITS01");
    }
}
