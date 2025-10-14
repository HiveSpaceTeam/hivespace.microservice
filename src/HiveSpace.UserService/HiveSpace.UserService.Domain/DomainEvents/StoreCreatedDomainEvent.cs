using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.UserService.Domain.DomainEvents;

public record StoreCreatedDomainEvent(Guid StoreId, Guid OwnerId) : IDomainEvent;