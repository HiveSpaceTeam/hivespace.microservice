using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HiveSpace.OrderService.Tests.Consumers.CheckoutSaga;

public class CommitCouponUsageConsumerTests
{
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly ICouponRepository _couponRepository = Substitute.For<ICouponRepository>();
    private readonly ILogger<CommitCouponUsageConsumer> _logger = Substitute.For<ILogger<CommitCouponUsageConsumer>>();
    private readonly CommitCouponUsageConsumer _consumer;

    public CommitCouponUsageConsumerTests()
    {
        _consumer = new CommitCouponUsageConsumer(_orderRepository, _couponRepository, _logger);
    }

    [Fact]
    public async Task Consume_WithNoUsages_RespondsWithSuccess()
    {
        var orderIds = new List<Guid> { Guid.NewGuid() };
        var message = new CommitCouponUsage { CorrelationId = Guid.NewGuid(), OrderIds = orderIds };
        var context = Substitute.For<ConsumeContext<CommitCouponUsage>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        _orderRepository.GetCouponUsageEntriesByOrderIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<OrderCouponUsageEntry>());

        await _consumer.Consume(context);

        await _couponRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await context.Received(1).RespondAsync<CouponUsageCommittedIntegrationEvent>(Arg.Any<object>());
    }

    [Fact]
    public async Task Consume_WhenCouponNotFound_RespondsWithFailure()
    {
        var orderId = Guid.NewGuid();
        var usage = new OrderCouponUsageEntry(orderId, Guid.NewGuid(), "DISCOUNT10", 10_000L, Currency.VND);
        var message = new CommitCouponUsage { CorrelationId = Guid.NewGuid(), OrderIds = [orderId] };
        var context = Substitute.For<ConsumeContext<CommitCouponUsage>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        _orderRepository.GetCouponUsageEntriesByOrderIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<OrderCouponUsageEntry> { usage });
        _couponRepository.GetByCodesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Coupon>());

        await _consumer.Consume(context);

        await context.Received(1).RespondAsync<CommitCouponUsageFailedIntegrationEvent>(Arg.Any<object>());
        await _couponRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
