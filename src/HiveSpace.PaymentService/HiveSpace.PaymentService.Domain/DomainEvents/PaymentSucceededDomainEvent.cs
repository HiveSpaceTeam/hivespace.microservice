using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.ValueObjects;

namespace HiveSpace.PaymentService.Domain.DomainEvents;

public record PaymentSucceededDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid BuyerId,
    Money Amount,
    string GatewayTransactionId,
    string IdempotencyKey) : IDomainEvent;
