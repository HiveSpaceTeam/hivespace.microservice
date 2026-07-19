using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.PaymentService.Application.Interfaces.Messaging;
using HiveSpace.PaymentService.Application.Payments.Commands.CreatePaymentAttempt;
using HiveSpace.PaymentService.Domain.Aggregates.Payments;
using HiveSpace.PaymentService.Domain.Aggregates.Payments.Enumerations;
using HiveSpace.PaymentService.Domain.Repositories;
using HiveSpace.PaymentService.Domain.Services;
using HiveSpace.Testing.Shared.Doubles;
using NSubstitute;
using PaymentAggregate = HiveSpace.PaymentService.Domain.Aggregates.Payments.Payment;
using Xunit;

namespace HiveSpace.PaymentService.Tests.Application.Payments;

public class CreatePaymentAttemptCommandHandlerTests
{
    [Fact]
    public async Task Handle_FailedVnPayPayment_CreatesNextAttemptWithSameReferenceNo()
    {
        var payment = CreatePayment();
        payment.MarkAttemptFailedOrExpired(payment.CurrentAttemptId!.Value, "Failed", "declined");
        var repository = Substitute.For<IPaymentRepository>();
        repository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);
        var gateway = Substitute.For<IPaymentGateway>();
        gateway.InitiatePaymentAsync(payment, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new GatewayInitiateResult("https://pay.test/retry", payment.ReferenceNo!));
        var gatewayFactory = Substitute.For<IPaymentGatewayFactory>();
        gatewayFactory.GetGateway(PaymentGateway.VNPay).Returns(gateway);
        var publisher = Substitute.For<IPaymentEventPublisher>();
        var handler = new CreatePaymentAttemptCommandHandler(repository, gatewayFactory, publisher, UserContextFor(payment));

        var result = await handler.Handle(
            new CreatePaymentAttemptCommand(
                payment.Id,
                "VNPAY",
                "retry-key",
                "https://shop.test/payment/return",
                "https://shop.test/payment/cancel"),
            CancellationToken.None);

        result.ReferenceNo.Should().Be(payment.ReferenceNo);
        result.Attempt.AttemptNo.Should().Be(2);
        result.Attempt.RedirectUrl.Should().Be("https://pay.test/retry");
        await gateway.Received(1).InitiatePaymentAsync(
            Arg.Is<PaymentAggregate>(p => p.CurrentAttempt!.AttemptNo == 2),
            "https://shop.test/payment/return",
            "https://shop.test/payment/cancel",
            Arg.Any<CancellationToken>());
        await publisher.Received(1).PublishPaymentAttemptInitiatedAsync(
            Arg.Is<PaymentAggregate>(p => p.CurrentAttempt!.AttemptNo == 2 && p.ReferenceNo == payment.ReferenceNo),
            Arg.Any<CancellationToken>());
        Received.InOrder(() =>
        {
            publisher.PublishPaymentAttemptInitiatedAsync(Arg.Any<PaymentAggregate>(), Arg.Any<CancellationToken>());
            repository.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_FailedVnPayBeforeFulfillment_CreatesOfflineCodAttempt()
    {
        var payment = CreatePayment();
        payment.MarkAttemptFailedOrExpired(payment.CurrentAttemptId!.Value, "Failed", "declined");
        var repository = Substitute.For<IPaymentRepository>();
        repository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);
        var gatewayFactory = Substitute.For<IPaymentGatewayFactory>();
        var publisher = Substitute.For<IPaymentEventPublisher>();
        var handler = new CreatePaymentAttemptCommandHandler(repository, gatewayFactory, publisher, UserContextFor(payment));

        var result = await handler.Handle(new CreatePaymentAttemptCommand(payment.Id, "COD", "cod-key"), CancellationToken.None);

        result.ReferenceNo.Should().Be(payment.ReferenceNo);
        result.Attempt.AttemptNo.Should().Be(2);
        result.Attempt.RedirectUrl.Should().BeNull();
        await publisher.Received(1).PublishPaymentAttemptInitiatedAsync(
            Arg.Is<PaymentAggregate>(p => p.CurrentAttempt!.AttemptNo == 2 && p.MethodCode == "COD"),
            Arg.Any<CancellationToken>());
        Received.InOrder(() =>
        {
            publisher.PublishPaymentAttemptInitiatedAsync(Arg.Any<PaymentAggregate>(), Arg.Any<CancellationToken>());
            repository.SaveChangesAsync(Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_LinkedOrderInFulfillment_RejectsRetry()
    {
        var payment = PaymentAggregate.CreateCheckout(
            Guid.NewGuid(),
            "PAY-01JZXYZABCDEABCDEABCDEABC",
            Guid.NewGuid(),
            Money.FromVND(100_000),
            "VNPAY",
            "idem-key",
            [new PaymentLinkedOrder(Guid.NewGuid(), "ORD-01JZXYZABCDEABCDEABCDEABC", Guid.NewGuid(), 100_000, "VND", "Confirmed")]);
        payment.MarkAttemptFailedOrExpired(payment.CurrentAttemptId!.Value, "Failed", "declined");
        var repository = Substitute.For<IPaymentRepository>();
        repository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);
        var publisher = Substitute.For<IPaymentEventPublisher>();
        var handler = new CreatePaymentAttemptCommandHandler(
            repository,
            Substitute.For<IPaymentGatewayFactory>(),
            publisher,
            UserContextFor(payment));

        var act = () => handler.Handle(new CreatePaymentAttemptCommand(payment.Id, "COD", "cod-key"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidFieldException>();
        await publisher.DidNotReceive().PublishPaymentAttemptInitiatedAsync(
            Arg.Any<PaymentAggregate>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AsDifferentBuyer_ThrowsForbiddenException()
    {
        var payment = CreatePayment();
        payment.MarkAttemptFailedOrExpired(payment.CurrentAttemptId!.Value, "Failed", "declined");
        var repository = Substitute.For<IPaymentRepository>();
        repository.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);
        var publisher = Substitute.For<IPaymentEventPublisher>();
        var handler = new CreatePaymentAttemptCommandHandler(
            repository,
            Substitute.For<IPaymentGatewayFactory>(),
            publisher,
            new FakeUserContext { UserId = Guid.NewGuid() });

        var act = () => handler.Handle(new CreatePaymentAttemptCommand(payment.Id, "COD", "cod-key"), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        await publisher.DidNotReceive().PublishPaymentAttemptInitiatedAsync(
            Arg.Any<PaymentAggregate>(),
            Arg.Any<CancellationToken>());
        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static PaymentAggregate CreatePayment()
    {
        return PaymentAggregate.CreateCheckout(
            Guid.NewGuid(),
            "PAY-01JZXYZABCDEABCDEABCDEABC",
            Guid.NewGuid(),
            Money.FromVND(100_000),
            "VNPAY",
            "idem-key",
            [new PaymentLinkedOrder(Guid.NewGuid(), "ORD-01JZXYZABCDEABCDEABCDEABC", Guid.NewGuid(), 100_000, "VND")]);
    }

    private static FakeUserContext UserContextFor(PaymentAggregate payment) => new()
    {
        UserId = payment.BuyerId
    };
}
