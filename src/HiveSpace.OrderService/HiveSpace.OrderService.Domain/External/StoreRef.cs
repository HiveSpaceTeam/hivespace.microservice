using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.OrderService.Domain.Enumerations;

namespace HiveSpace.OrderService.Domain.External;

public class StoreRef : IAuditable
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public SellerStatus Status { get; private set; }

    /// <summary>UserId of the store owner. Used to resolve seller for notification delivery.</summary>
    public Guid OwnerId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private StoreRef() { }

    public StoreRef(Guid id, string name, SellerStatus status, Guid ownerId)
    {
        Id = id;
        Name = name;
        Status = status;
        OwnerId = ownerId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(string name, SellerStatus status)
    {
        Name = name;
        Status = status;
    }
}
