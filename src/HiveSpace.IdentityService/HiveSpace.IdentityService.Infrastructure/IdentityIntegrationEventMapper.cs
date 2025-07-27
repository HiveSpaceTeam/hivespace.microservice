using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Infrastructure.Messaging.Events;
using HiveSpace.Infrastructure.Messaging.Interfaces;


namespace HiveSpace.IdentityService.Infrastructure;
public class IdentityIntegrationEventMapper : IIntegrationEventMapper
{
    public List<IntegrationEvent> Map(IReadOnlyCollection<IDomainEvent> domainEvents)
    {
        return new List<IntegrationEvent> { new IntegrationEvent() };
    }
}
