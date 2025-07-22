using System.Text.Json;
using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.Errors;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.Infrastructure.Persistence.Outbox;

/// <summary>
/// Implements IOutboxRepository to add domain events to the OutboxMessage table.
/// </summary>
public class OutboxRepository : IOutboxRepository
{
    private readonly DbContext _dbContext;

    public OutboxRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void AddDomainEvent<TEvent>(TEvent domainEvent) where TEvent : class, IDomainEvent
    {
        var outboxMessage = new OutboxMessage(
            Guid.NewGuid(),
            DateTime.UtcNow,
            domainEvent.GetType().FullName!,
            JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions { WriteIndented = true }));

        _dbContext.Set<OutboxMessage>().Add(outboxMessage);
    }

    public void AddOutboxMessage(OutboxMessage outboxMessage)
    {
        _dbContext.Set<OutboxMessage>().Add(outboxMessage);
    }
} 