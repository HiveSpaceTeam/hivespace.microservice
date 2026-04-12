using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.ValueObjects;

namespace HiveSpace.PaymentService.Domain.DomainEvents;

public record PaymentInitiatedDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid BuyerId,
    Money Amount) : IDomainEvent;

public record PaymentProcessingDomainEvent(
    Guid PaymentId,
    Guid OrderId) : IDomainEvent;

public record PaymentSucceededDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid BuyerId,
    Money Amount,
    string GatewayTransactionId,
    string IdempotencyKey) : IDomainEvent;

public record PaymentFailedDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid BuyerId,
    string Reason) : IDomainEvent;

public record PaymentCancelledDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid BuyerId) : IDomainEvent;

public record PaymentExpiredDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid BuyerId) : IDomainEvent;
