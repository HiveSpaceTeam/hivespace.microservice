using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.PaymentService.Domain.DomainEvents;

public record PaymentExpiredDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid BuyerId) : IDomainEvent;
