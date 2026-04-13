using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.ValueObjects;

namespace HiveSpace.PaymentService.Domain.DomainEvents;

public record WalletDebitedDomainEvent(
    Guid WalletId,
    Guid UserId,
    Money Amount,
    string Reference) : IDomainEvent;
