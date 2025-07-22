using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.Domain.Shared.Entities;
public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot

    where TKey : struct, IEquatable<TKey>
{
    private List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent eventItem)
    {
        _domainEvents = _domainEvents ?? [];
        _domainEvents.Add(eventItem);
    }

    public void RemoveDomainEvent(IDomainEvent eventItem)
    {
        _domainEvents?.Remove(eventItem);
    }

    public void ClearDomainEvents()
    {
        _domainEvents?.Clear();
    }
}
