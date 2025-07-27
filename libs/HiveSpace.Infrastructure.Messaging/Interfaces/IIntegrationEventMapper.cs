using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Infrastructure.Messaging.Events;

namespace HiveSpace.Infrastructure.Messaging.Interfaces;
public interface IIntegrationEventMapper
{
    List<IntegrationEvent> Map(IReadOnlyCollection<IDomainEvent> domainEvents);
}
