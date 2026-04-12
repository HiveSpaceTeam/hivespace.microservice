using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.ValueObjects;

namespace HiveSpace.PaymentService.Domain.DomainEvents;

public record WalletCreatedDomainEvent(
    Guid WalletId,
    Guid UserId) : IDomainEvent;

public record WalletCreditedDomainEvent(
    Guid WalletId,
    Guid UserId,
    Money Amount,
    string Reference) : IDomainEvent;

public record WalletDebitedDomainEvent(
    Guid WalletId,
    Guid UserId,
    Money Amount,
    string Reference) : IDomainEvent;
