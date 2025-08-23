using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.UserService.Domain.DomainEvents;

public class UserBecameCustomerEvent : IDomainEvent
{
    public Guid UserId { get; }
    
    public UserBecameCustomerEvent(Guid userId)
    {
        UserId = userId;
    }
}
