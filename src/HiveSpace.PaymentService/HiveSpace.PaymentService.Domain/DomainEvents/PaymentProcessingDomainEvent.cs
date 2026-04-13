using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.PaymentService.Domain.DomainEvents;

public record PaymentProcessingDomainEvent(
    Guid PaymentId,
    Guid OrderId) : IDomainEvent;
