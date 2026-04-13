using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.PaymentService.Domain.DomainEvents;

public record PaymentCancelledDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid BuyerId) : IDomainEvent;
