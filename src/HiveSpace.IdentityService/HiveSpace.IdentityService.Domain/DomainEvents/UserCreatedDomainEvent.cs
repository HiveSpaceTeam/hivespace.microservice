using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.IdentityService.Domain.DomainEvents;

public record UserCreatedDomainEvent(Guid UserId, string Email, string FullName) : IDomainEvent; 