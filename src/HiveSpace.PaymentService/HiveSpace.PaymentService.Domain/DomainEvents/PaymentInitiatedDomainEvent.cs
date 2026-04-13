using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.ValueObjects;

namespace HiveSpace.PaymentService.Domain.DomainEvents;

public record PaymentInitiatedDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid BuyerId,
    Money Amount) : IDomainEvent;
