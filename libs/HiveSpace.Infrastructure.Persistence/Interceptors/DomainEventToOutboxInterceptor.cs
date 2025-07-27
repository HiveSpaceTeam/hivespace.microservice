using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using HiveSpace.Infrastructure.Messaging.Interfaces;

namespace HiveSpace.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core SaveChangesInterceptor that extracts domain events from Aggregate Roots
/// and adds them to the Outbox for reliable publishing.
/// </summary>
public class DomainEventToOutboxInterceptor : ISaveChangesInterceptor
{
    private readonly IIntegrationEventMapper _integrationEventMapper;

    public DomainEventToOutboxInterceptor(
        IIntegrationEventMapper integrationEventMapper
        )
    {
        _integrationEventMapper = integrationEventMapper ?? throw new ArgumentNullException(nameof(integrationEventMapper));
    }

    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddDomainEventsToOutbox(eventData.Context);
        return result;
    }

    public ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AddDomainEventsToOutbox(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void AddDomainEventsToOutbox(DbContext? context)
    {
        if (context == null) return;

        // Get all aggregate roots that are being added or modified
        var aggregateRoots = context.ChangeTracker.Entries<IAggregateRoot>()
            .Where(entry => entry.State == EntityState.Added || entry.State == EntityState.Modified)
            .Select(entry => entry.Entity)
            .ToList();

        foreach (var aggregateRoot in aggregateRoots)
        {
            var domainEvents = aggregateRoot.DomainEvents.ToList(); // Get a copy of events
            if (!domainEvents.Any())
            {
                return;
            }
            var integrationEvents = _integrationEventMapper.Map(domainEvents);
            if (!integrationEvents.Any())
            {
                return;
            }
            // Add events directly to the DbContext to avoid circular dependency
            foreach (var integrationEvent in integrationEvents)
            {
                var outboxMessage = new OutboxMessage(integrationEvent, Guid.NewGuid());
                context.Set<OutboxMessage>().Add(outboxMessage);
            }
            aggregateRoot.ClearDomainEvents(); // Clear events from the aggregate root after adding to outbox
        }
    }
}