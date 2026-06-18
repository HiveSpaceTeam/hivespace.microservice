using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Commands;
using HiveSpace.Infrastructure.Messaging.Shared.CheckoutSaga.Events;
using HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;
using HiveSpace.OrderService.Domain.Aggregates.Orders;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.OrderService.Domain.ValueObjects;
using HiveSpace.OrderService.Tests.Domain;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HiveSpace.OrderService.Tests.Consumers.CheckoutSaga;

public class MarkOrderAsCODConsumerTests
{
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly ILogger<MarkOrderAsCODConsumer> _logger = Substitute.For<ILogger<MarkOrderAsCODConsumer>>();
    private readonly MarkOrderAsCODConsumer _consumer;

    public MarkOrderAsCODConsumerTests()
    {
        OrderIdGeneratorFixture.EnsureInitialized();
        _consumer = new MarkOrderAsCODConsumer(_orderRepository, _logger);
    }

    private static DeliveryAddress ValidAddress() =>
        new("Test User", new PhoneNumber("0901234567"), "123 Main St", "Ward 1", "Hanoi");

    [Fact]
    public async Task Consume_WhenOrderNotFound_RespondsWithFailure()
    {
        var missingId = Guid.NewGuid();
        var message = new MarkOrderAsCOD { CorrelationId = Guid.NewGuid(), OrderIds = [missingId] };
        var context = Substitute.For<ConsumeContext<MarkOrderAsCOD>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        _orderRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Order>());

        await _consumer.Consume(context);

        await context.Received(1).RespondAsync<MarkOrderAsCODFailedIntegrationEvent>(Arg.Any<object>());
        await _orderRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenOrdersFound_MarksAsCODAndRespondsWithSuccess()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        var message = new MarkOrderAsCOD { CorrelationId = Guid.NewGuid(), OrderIds = [order.Id] };
        var context = Substitute.For<ConsumeContext<MarkOrderAsCOD>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        _orderRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Order> { order });

        await _consumer.Consume(context);

        await _orderRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await context.Received(1).RespondAsync<OrderMarkedAsCODIntegrationEvent>(Arg.Any<object>());
    }

    [Fact]
    public async Task Consume_WhenOrderInInvalidStatus_RespondsWithFailure()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.MarkAsExpired();
        var message = new MarkOrderAsCOD { CorrelationId = Guid.NewGuid(), OrderIds = [order.Id] };
        var context = Substitute.For<ConsumeContext<MarkOrderAsCOD>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        _orderRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Order> { order });

        await _consumer.Consume(context);

        await context.Received(1).RespondAsync<MarkOrderAsCODFailedIntegrationEvent>(Arg.Any<object>());
        await _orderRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenOrderAlreadyMarkedAsCOD_SkipsMarkAndRespondsWithSuccess()
    {
        var order = Order.Create(Guid.NewGuid(), ValidAddress(), Guid.NewGuid());
        order.MarkAsCOD();
        var message = new MarkOrderAsCOD { CorrelationId = Guid.NewGuid(), OrderIds = [order.Id] };
        var context = Substitute.For<ConsumeContext<MarkOrderAsCOD>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        _orderRepository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Order> { order });

        await _consumer.Consume(context);

        await _orderRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await context.Received(1).RespondAsync<OrderMarkedAsCODIntegrationEvent>(Arg.Any<object>());
    }
}
