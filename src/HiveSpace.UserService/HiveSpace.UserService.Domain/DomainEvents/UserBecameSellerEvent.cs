using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.UserService.Domain.DomainEvents;

public class UserBecameSellerEvent : IDomainEvent
{
    public Guid UserId { get; }
    public Guid StoreId { get; }
    
    public UserBecameSellerEvent(Guid userId, Guid storeId)
    {
        UserId = userId;
        StoreId = storeId;
    }
}
