﻿using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.Domain.Shared.Interfaces;

public interface IRepository<TEntity> where TEntity : IAggregateRoot
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    void Add(TEntity entity);
    Task<List<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(object id, bool includeDetail = false);
    void UpdateWithConcurrency<T>(T entity, DateTimeOffset originalDateTimeUpdated, DateTimeOffset newDateTimeUpdated) where T : class, TEntity, IAuditable;
    void UpdateWithConcurrency<T>(T entity, DateTimeOffset originalDateTimeUpdated) where T : class, TEntity, IAuditable;
}
