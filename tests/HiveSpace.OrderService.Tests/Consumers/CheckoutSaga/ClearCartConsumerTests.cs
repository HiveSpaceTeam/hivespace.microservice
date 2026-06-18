using HiveSpace.OrderService.Api.Consumers.Saga.CheckoutSaga;
using HiveSpace.OrderService.Application.Contracts;
using HiveSpace.OrderService.Domain.Aggregates.Carts;
using HiveSpace.OrderService.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HiveSpace.OrderService.Tests.Consumers.CheckoutSaga;

public class ClearCartConsumerTests
{
    private readonly ICartRepository _cartRepository = Substitute.For<ICartRepository>();
    private readonly ILogger<ClearCartConsumer> _logger = Substitute.For<ILogger<ClearCartConsumer>>();
    private readonly ClearCartConsumer _consumer;

    public ClearCartConsumerTests()
    {
        HiveSpace.OrderService.Tests.Domain.OrderIdGeneratorFixture.EnsureInitialized();
        _consumer = new ClearCartConsumer(_cartRepository, _logger);
    }

    [Fact]
    public async Task Consume_WhenCartNotFound_RespondsWithCartCleared()
    {
        var message = new ClearCart { CorrelationId = Guid.NewGuid(), UserId = Guid.NewGuid(), PurchasedStoreIds = [] };
        var context = Substitute.For<ConsumeContext<ClearCart>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        _cartRepository.GetByUserIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Cart?)null);

        await _consumer.Consume(context);

        await context.Received(1).RespondAsync<CartCleared>(Arg.Any<object>());
        await _cartRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WhenCartFoundAndNotEmpty_ClearsAndSavesAndResponds()
    {
        var userId = Guid.NewGuid();
        var cart = Cart.Create(userId, id: Guid.NewGuid());
        cart.AddItem(1L, 1L, 2);
        var message = new ClearCart { CorrelationId = Guid.NewGuid(), UserId = userId, PurchasedStoreIds = [] };
        var context = Substitute.For<ConsumeContext<ClearCart>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        _cartRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(cart);

        await _consumer.Consume(context);

        await _cartRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await context.Received(1).RespondAsync<CartCleared>(Arg.Any<object>());
    }

    [Fact]
    public async Task Consume_WhenCartExistsButIsEmpty_SkipsClearAndResponds()
    {
        var userId = Guid.NewGuid();
        var cart = Cart.Create(userId, id: Guid.NewGuid());
        var message = new ClearCart { CorrelationId = Guid.NewGuid(), UserId = userId, PurchasedStoreIds = [] };
        var context = Substitute.For<ConsumeContext<ClearCart>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(CancellationToken.None);
        _cartRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(cart);

        await _consumer.Consume(context);

        await _cartRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await context.Received(1).RespondAsync<CartCleared>(Arg.Any<object>());
    }
}
