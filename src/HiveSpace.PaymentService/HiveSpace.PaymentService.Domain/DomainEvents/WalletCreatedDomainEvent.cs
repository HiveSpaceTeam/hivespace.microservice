using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.PaymentService.Domain.DomainEvents;

public record WalletCreatedDomainEvent(
    Guid WalletId,
    Guid UserId) : IDomainEvent;
