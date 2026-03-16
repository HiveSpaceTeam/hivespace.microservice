using HiveSpace.Domain.Shared.Specifications;

namespace HiveSpace.Domain.Shared.Interfaces;

public interface IRepository<TEntity> where TEntity : class, IAggregateRoot
{
    IQueryable<TEntity> GetQueryable();
    Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetListAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetPagedAsync(Specification<TEntity> specification, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdAsync(object id, bool includeDetail = false, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    void Add(TEntity entity);
    void Remove(TEntity entity);
    void UpdateWithConcurrency<T>(T entity, DateTimeOffset originalDateTimeUpdated, DateTimeOffset newDateTimeUpdated) where T : class, TEntity, IAuditable;
    void UpdateWithConcurrency<T>(T entity, DateTimeOffset originalDateTimeUpdated) where T : class, TEntity, IAuditable;
}

