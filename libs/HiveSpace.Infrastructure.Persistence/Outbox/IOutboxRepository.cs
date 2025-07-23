using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.Infrastructure.Persistence.Outbox;

/// <summary>
/// Interface for adding messages to the outbox.
/// This is typically called from within a business transaction.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Adds a domain event to the outbox table.
    /// </summary>
    /// <typeparam name="TEvent">The type of the domain event.</typeparam>
    /// <param name="domainEvent">The domain event instance.</param>
    void AddDomainEvent<TEvent>(TEvent domainEvent) where TEvent : class, IDomainEvent;

    /// <summary>
    /// Adds an outbox message directly to the outbox table.
    /// </summary>
    /// <param name="outboxMessage">The outbox message to add.</param>
    void AddOutboxMessage(OutboxMessage outboxMessage);
} 