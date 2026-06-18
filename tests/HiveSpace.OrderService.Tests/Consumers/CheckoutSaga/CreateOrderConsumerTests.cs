using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Dtos;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Enumerations;
using HiveSpace.OrderService.Domain.External;
using HiveSpace.OrderService.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HiveSpace.OrderService.Tests.Consumers.CheckoutSaga;

public class CreateOrderConsumerTests
{
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly ICartRepository _cartRepository = Substitute.For<ICartRepository>();
    private readonly IProductRefRepository _productRefRepository = Substitute.For<IProductRefRepository>();
    private readonly ISkuRefRepository _skuRefRepository = Substitute.For<ISkuRefRepository>();
    private readonly ICouponRepository _couponRepository = Substitute.For<ICouponRepository>();
    private readonly ILogger<CreateOrderConsumer> _logger = Substitute.For<ILogger<CreateOrderConsumer>>();
    private readonly CreateOrderConsumer _consumer;

    public CreateOrderConsumerTests()
    {
        HiveSpace.OrderService.Tests.Domain.OrderIdGeneratorFixture.EnsureInitialized();
        _consumer = new CreateOrderConsumer(
            _orderRepository,
            _cartRepository,
            _productRefRepository,
            _skuRefRepository,
            _couponRepository,
            _logger);
    }

    [Fact]
    public async Task Consume_WhenCartIsNull_RespondsWithFailure()
    {
        var message = new CreateOrder
        {
            CorrelationId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            DeliveryAddress = new DeliveryAddressDto { RecipientName = "User", Phone = "0901234567", StreetAddress = "123 St", Commune = "Ward 1", Province = "Hanoi" }
        };
        var context = Substitute.For<ConsumeContext<CreateOrder>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        _cartRepository.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Cart?)null);

        await _consumer.Consume(context);

        await context.Received(1).RespondAsync<OrderCreationFailedIntegrationEvent>(Arg.Any<object>());
    }

    [Fact]
    public async Task Consume_WhenProductsNotFound_RespondsWithFailure()
    {
        var userId = Guid.NewGuid();
        var cart = Cart.Create(userId, id: Guid.NewGuid());
        cart.AddItem(1L, 1L, 1);
        var message = new CreateOrder
        {
            CorrelationId = Guid.NewGuid(),
            UserId = userId,
            DeliveryAddress = BuildAddress()
        };
        var context = Substitute.For<ConsumeContext<CreateOrder>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        _cartRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(cart);
        _productRefRepository.GetByIdsAsync(Arg.Any<IEnumerable<long>>(), Arg.Any<CancellationToken>())
            .Returns(new List<ProductRef>());
        _skuRefRepository.GetByIdsAsync(Arg.Any<IEnumerable<long>>(), Arg.Any<CancellationToken>())
            .Returns(new List<SkuRef>());

        await _consumer.Consume(context);

        await context.Received(1).RespondAsync<OrderCreationFailedIntegrationEvent>(Arg.Any<object>());
    }

    [Fact]
    public async Task Consume_WhenSkusMissing_RespondsWithFailure()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var cart = Cart.Create(userId, id: Guid.NewGuid());
        cart.AddItem(1L, 1L, 1);
        var message = new CreateOrder { CorrelationId = Guid.NewGuid(), UserId = userId, DeliveryAddress = BuildAddress() };
        var context = BuildContext(message);
        _cartRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(cart);
        _productRefRepository.GetByIdsAsync(Arg.Any<IEnumerable<long>>(), Arg.Any<CancellationToken>())
            .Returns(new List<ProductRef> { new(1L, storeId, "Widget", null, ProductStatus.Available) });
        _skuRefRepository.GetByIdsAsync(Arg.Any<IEnumerable<long>>(), Arg.Any<CancellationToken>())
            .Returns(new List<SkuRef>());

        await _consumer.Consume(context);

        await context.Received(1).RespondAsync<OrderCreationFailedIntegrationEvent>(Arg.Any<object>());
    }

    [Fact]
    public async Task Consume_WhenRequestedCouponsNotFound_RespondsWithFailure()
    {
        var (userId, storeId, cart) = BuildCartWithItem();
        var message = new CreateOrder
        {
            CorrelationId = Guid.NewGuid(), UserId = userId, DeliveryAddress = BuildAddress(),
            CouponSelections = new CheckoutCouponSelectionDto { PlatformCouponCodes = ["SAVE10"] }
        };
        var context = BuildContext(message);
        SetupRefsFound(userId, storeId, cart);
        _couponRepository.GetByCodesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Coupon>());

        await _consumer.Consume(context);

        await context.Received(1).RespondAsync<OrderCreationFailedIntegrationEvent>(Arg.Any<object>());
    }

    [Fact]
    public async Task Consume_WhenStoreCouponAppliedToNonCheckoutStore_RespondsWithFailure()
    {
        var (userId, storeId, cart) = BuildCartWithItem();
        var otherStoreId = Guid.NewGuid();
        var message = new CreateOrder
        {
            CorrelationId = Guid.NewGuid(), UserId = userId, DeliveryAddress = BuildAddress(),
            CouponSelections = new CheckoutCouponSelectionDto
            {
                StoreCoupons = [new StoreCouponSelectionDto(otherStoreId, "OTHERCOUPON")]
            }
        };
        var context = BuildContext(message);
        SetupRefsFound(userId, storeId, cart);
        _couponRepository.GetByCodesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Coupon> { BuildPlatformCoupon("OTHERCOUPON") });

        await _consumer.Consume(context);

        await context.Received(1).RespondAsync<OrderCreationFailedIntegrationEvent>(Arg.Any<object>());
    }

    [Fact]
    public async Task Consume_WhenPlatformCouponCodeBelongsToStore_RespondsWithFailure()
    {
        var (userId, storeId, cart) = BuildCartWithItem();
        var message = new CreateOrder
        {
            CorrelationId = Guid.NewGuid(), UserId = userId, DeliveryAddress = BuildAddress(),
            CouponSelections = new CheckoutCouponSelectionDto { PlatformCouponCodes = ["STOREC"] }
        };
        var context = BuildContext(message);
        SetupRefsFound(userId, storeId, cart);
        var storeCoupon = Coupon.CreateByStore(storeId, Guid.NewGuid(), "STOREC", "Store Only",
            DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30), id: Guid.NewGuid());
        _couponRepository.GetByCodesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Coupon> { storeCoupon });

        await _consumer.Consume(context);

        await context.Received(1).RespondAsync<OrderCreationFailedIntegrationEvent>(Arg.Any<object>());
    }

    [Fact]
    public async Task Consume_WhenPlatformCouponValidationFails_RespondsWithFailure()
    {
        var (userId, storeId, cart) = BuildCartWithItem();
        var message = new CreateOrder
        {
            CorrelationId = Guid.NewGuid(), UserId = userId, DeliveryAddress = BuildAddress(),
            CouponSelections = new CheckoutCouponSelectionDto { PlatformCouponCodes = ["HIGHMIN"] }
        };
        var context = BuildContext(message);
        SetupRefsFound(userId, storeId, cart);
        var coupon = Coupon.CreateByPlatform("admin", "HIGHMIN", "High Min",
            DiscountType.FixedAmount, null, Money.FromVND(10_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30),
            minOrderAmount: Money.FromVND(10_000_000), id: Guid.NewGuid());
        _couponRepository.GetByCodesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Coupon> { coupon });

        await _consumer.Consume(context);

        await context.Received(1).RespondAsync<OrderCreationFailedIntegrationEvent>(Arg.Any<object>());
    }

    [Fact]
    public async Task Consume_WhenStoreCouponOwnerTypeMismatch_RespondsWithFailure()
    {
        var (userId, storeId, cart) = BuildCartWithItem();
        var message = new CreateOrder
        {
            CorrelationId = Guid.NewGuid(), UserId = userId, DeliveryAddress = BuildAddress(),
            CouponSelections = new CheckoutCouponSelectionDto
            {
                StoreCoupons = [new StoreCouponSelectionDto(storeId, "PLATCOUPON")]
            }
        };
        var context = BuildContext(message);
        SetupRefsFound(userId, storeId, cart);
        _couponRepository.GetByCodesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Coupon> { BuildPlatformCoupon("PLATCOUPON") });

        await _consumer.Consume(context);

        await context.Received(1).RespondAsync<OrderCreationFailedIntegrationEvent>(Arg.Any<object>());
    }

    [Fact]
    public async Task Consume_WhenStoreCouponEvaluationFails_RespondsWithFailure()
    {
        var (userId, storeId, cart) = BuildCartWithItem();
        var message = new CreateOrder
        {
            CorrelationId = Guid.NewGuid(), UserId = userId, DeliveryAddress = BuildAddress(),
            CouponSelections = new CheckoutCouponSelectionDto
            {
                StoreCoupons = [new StoreCouponSelectionDto(storeId, "STOREMIN")]
            }
        };
        var context = BuildContext(message);
        SetupRefsFound(userId, storeId, cart);
        var coupon = Coupon.CreateByStore(storeId, Guid.NewGuid(), "STOREMIN", "Store High Min",
            DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30),
            minOrderAmount: Money.FromVND(10_000_000), id: Guid.NewGuid());
        _couponRepository.GetByCodesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Coupon> { coupon });

        await _consumer.Consume(context);

        await context.Received(1).RespondAsync<OrderCreationFailedIntegrationEvent>(Arg.Any<object>());
    }

    [Fact]
    public async Task Consume_WhenStoreCouponBelongsToDifferentStore_RespondsWithFailure()
    {
        var (userId, storeId, cart) = BuildCartWithItem();
        var differentStoreId = Guid.NewGuid();
        // Coupon is a valid store coupon but its StoreId belongs to a different store than the selection targets
        var coupon = Coupon.CreateByStore(differentStoreId, Guid.NewGuid(), "WRONGSTORE", "Wrong Store Coupon",
            DiscountType.FixedAmount, null, Money.FromVND(3_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30), id: Guid.NewGuid());
        var message = new CreateOrder
        {
            CorrelationId = Guid.NewGuid(), UserId = userId, DeliveryAddress = BuildAddress(),
            CouponSelections = new CheckoutCouponSelectionDto
            {
                StoreCoupons = [new StoreCouponSelectionDto(storeId, "WRONGSTORE")]
            }
        };
        var context = BuildContext(message);
        SetupRefsFound(userId, storeId, cart);
        _couponRepository.GetByCodesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Coupon> { coupon });

        await _consumer.Consume(context);

        await context.Received(1).RespondAsync<OrderCreationFailedIntegrationEvent>(Arg.Any<object>());
    }

    [Fact]
    public async Task Consume_HappyPath_CreatesOrderAndRespondsWithSuccess()
    {
        var (userId, storeId, cart) = BuildCartWithItem();
        var message = new CreateOrder
        {
            CorrelationId = Guid.NewGuid(), UserId = userId,
            DeliveryAddress = BuildAddress(),
            PaymentMethod = PaymentMethod.COD
        };
        var context = BuildContext(message);
        SetupRefsFound(userId, storeId, cart);

        await _consumer.Consume(context);

        await context.Received(1).RespondAsync<OrderCreatedIntegrationEvent>(Arg.Any<object>());
        await _orderRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_HappyPath_WithPlatformCoupon_AppliesProration()
    {
        var (userId, storeId, cart) = BuildCartWithItem();
        var coupon = BuildPlatformCoupon("SAVE5K");
        var message = new CreateOrder
        {
            CorrelationId = Guid.NewGuid(), UserId = userId,
            DeliveryAddress = BuildAddress(),
            PaymentMethod = PaymentMethod.COD,
            CouponSelections = new CheckoutCouponSelectionDto { PlatformCouponCodes = ["SAVE5K"] }
        };
        var context = BuildContext(message);
        SetupRefsFound(userId, storeId, cart);
        _couponRepository.GetByCodesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Coupon> { coupon });

        await _consumer.Consume(context);

        await context.Received(1).RespondAsync<OrderCreatedIntegrationEvent>(Arg.Any<object>());
        await _orderRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_HappyPath_WithPlatformCouponTwoStores_CoversRemainder()
    {
        // FixedAmount=1 across 2 equal-value orders: BaseAmount=0 for both, one gets the 1 VND remainder
        // and the other gets 0 — covering both remainingAmount>0 and the allocated<=0 continue branch
        var userId = Guid.NewGuid();
        var storeId1 = Guid.NewGuid();
        var storeId2 = Guid.NewGuid();
        var cart = Cart.Create(userId, id: Guid.NewGuid());
        cart.AddItem(1L, 1L, 1);
        cart.AddItem(2L, 2L, 1);
        var coupon = Coupon.CreateByPlatform("admin", "TINY1", "1 VND Coupon",
            DiscountType.FixedAmount, null, Money.FromVND(1), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30), id: Guid.NewGuid());
        var message = new CreateOrder
        {
            CorrelationId = Guid.NewGuid(), UserId = userId,
            DeliveryAddress = BuildAddress(),
            PaymentMethod = PaymentMethod.COD,
            CouponSelections = new CheckoutCouponSelectionDto { PlatformCouponCodes = ["TINY1"] }
        };
        var context = BuildContext(message);
        _cartRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(cart);
        _productRefRepository.GetByIdsAsync(Arg.Any<IEnumerable<long>>(), Arg.Any<CancellationToken>())
            .Returns(new List<ProductRef>
            {
                new(1L, storeId1, "Widget1", null, ProductStatus.Available),
                new(2L, storeId2, "Widget2", null, ProductStatus.Available)
            });
        _skuRefRepository.GetByIdsAsync(Arg.Any<IEnumerable<long>>(), Arg.Any<CancellationToken>())
            .Returns(new List<SkuRef>
            {
                new(1L, 1L, "SKU-001", 50_000L, "VND", null, null),
                new(2L, 2L, "SKU-002", 50_000L, "VND", null, null)
            });
        _couponRepository.GetByCodesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Coupon> { coupon });

        await _consumer.Consume(context);

        await context.Received(1).RespondAsync<OrderCreatedIntegrationEvent>(Arg.Any<object>());
        await _orderRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_HappyPath_WithStoreCoupon_AppliesDiscount()
    {
        var (userId, storeId, cart) = BuildCartWithItem();
        var coupon = Coupon.CreateByStore(storeId, Guid.NewGuid(), "STORE3K", "Store 3K",
            DiscountType.FixedAmount, null, Money.FromVND(3_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30), id: Guid.NewGuid());
        var message = new CreateOrder
        {
            CorrelationId = Guid.NewGuid(), UserId = userId,
            DeliveryAddress = BuildAddress(),
            PaymentMethod = PaymentMethod.COD,
            CouponSelections = new CheckoutCouponSelectionDto
            {
                StoreCoupons = [new StoreCouponSelectionDto(storeId, "STORE3K")]
            }
        };
        var context = BuildContext(message);
        SetupRefsFound(userId, storeId, cart);
        _couponRepository.GetByCodesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Coupon> { coupon });

        await _consumer.Consume(context);

        await context.Received(1).RespondAsync<OrderCreatedIntegrationEvent>(Arg.Any<object>());
        await _orderRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static DeliveryAddressDto BuildAddress() => new()
    {
        RecipientName = "Test User", Phone = "0901234567",
        StreetAddress = "123 St", Commune = "Ward 1", Province = "Hanoi"
    };

    private static ConsumeContext<CreateOrder> BuildContext(CreateOrder message)
    {
        var ctx = Substitute.For<ConsumeContext<CreateOrder>>();
        ctx.Message.Returns(message);
        ctx.CancellationToken.Returns(CancellationToken.None);
        return ctx;
    }

    private (Guid userId, Guid storeId, Cart cart) BuildCartWithItem()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var cart = Cart.Create(userId, id: Guid.NewGuid());
        cart.AddItem(1L, 1L, 1);
        return (userId, storeId, cart);
    }

    private void SetupRefsFound(Guid userId, Guid storeId, Cart cart)
    {
        _cartRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(cart);
        _productRefRepository.GetByIdsAsync(Arg.Any<IEnumerable<long>>(), Arg.Any<CancellationToken>())
            .Returns(new List<ProductRef> { new(1L, storeId, "Widget", null, ProductStatus.Available) });
        _skuRefRepository.GetByIdsAsync(Arg.Any<IEnumerable<long>>(), Arg.Any<CancellationToken>())
            .Returns(new List<SkuRef> { new(1L, 1L, "SKU-001", 50_000L, "VND", null, null) });
    }

    private static Coupon BuildPlatformCoupon(string code) =>
        Coupon.CreateByPlatform("admin", code, "Test Coupon",
            DiscountType.FixedAmount, null, Money.FromVND(5_000), CouponScope.ItemPrice,
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30), id: Guid.NewGuid());
}
