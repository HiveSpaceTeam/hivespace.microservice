using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HiveSpace.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core SaveChangesInterceptor that extracts domain events from Aggregate Roots
/// and adds them to the Outbox for reliable publishing.
/// </summary>
public class DomainEventToOutboxInterceptor : SaveChangesInterceptor
{
    private readonly IOutboxRepository _outboxRepository;

    public DomainEventToOutboxInterceptor(IOutboxRepository outboxRepository)
    {
        _outboxRepository = outboxRepository;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddDomainEventsToOutbox(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AddDomainEventsToOutbox(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
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
            // Cast to the concrete type to access DomainEvents
            if (aggregateRoot is AggregateRoot<Guid> typedAggregateRoot)
            {
                var domainEvents = typedAggregateRoot.DomainEvents.ToList(); // Get a copy of events
                foreach (var domainEvent in domainEvents)
                {
                    _outboxRepository.AddDomainEvent(domainEvent);
                }
                typedAggregateRoot.ClearDomainEvents(); // Clear events from the aggregate root after adding to outbox
            }
        }
    }
} 