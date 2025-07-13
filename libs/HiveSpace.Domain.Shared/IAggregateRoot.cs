using MediatR;

namespace HiveSpace.Domain.Shared;

public interface IAggregateRoot 
{
    void AddDomainEvent(IDomainEvent eventItem);

    void RemoveDomainEvent(IDomainEvent eventItem);

    void ClearDomainEvents();
}
