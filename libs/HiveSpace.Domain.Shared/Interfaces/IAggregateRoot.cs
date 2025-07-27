using MediatR;

namespace HiveSpace.Domain.Shared.Interfaces;

public interface IAggregateRoot 
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void AddDomainEvent(IDomainEvent eventItem);

    void RemoveDomainEvent(IDomainEvent eventItem);

    void ClearDomainEvents();
}
