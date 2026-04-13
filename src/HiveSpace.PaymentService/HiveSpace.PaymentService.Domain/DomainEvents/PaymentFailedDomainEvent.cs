using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.PaymentService.Domain.DomainEvents;

public record PaymentFailedDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid BuyerId,
    string Reason) : IDomainEvent;
