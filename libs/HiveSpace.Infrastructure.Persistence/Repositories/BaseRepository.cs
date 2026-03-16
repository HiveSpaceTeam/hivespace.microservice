using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.Specifications;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.Infrastructure.Persistence.Repositories;

public abstract class BaseRepository<TEntity, TKey>(DbContext context) : IRepository<TEntity>
    where TEntity : class, IAggregateRoot
    where TKey : struct, IEquatable<TKey>
{
    protected readonly DbContext _context = context;
    protected readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public IQueryable<TEntity> GetQueryable()
    {
        return _dbSet.AsNoTracking();
    }

    public void Add(TEntity entity)
    {
        _dbSet.Add(entity);
    }

    public void Remove(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<List<TEntity>> GetListAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(specification.ToExpression())
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(specification.ToExpression())
            .CountAsync(cancellationToken);
    }

    public async Task<List<TEntity>> GetPagedAsync(
        Specification<TEntity> specification,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(specification.ToExpression())
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    protected virtual IQueryable<TEntity> ApplyIncludeDetail(IQueryable<TEntity> query)
    {
        return query; // Default implementation returns query without includes
    }

    public async Task<TEntity?> GetByIdAsync(object id, bool includeDetail = false, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        if (includeDetail)
        {
            query = ApplyIncludeDetail(query);
        }

        return await query.FirstOrDefaultAsync(entity => EF.Property<object>(entity, "Id") == id, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void UpdateWithConcurrency<T>(T entity, DateTimeOffset originalDateTimeUpdated, DateTimeOffset newDateTimeUpdated) where T : class, TEntity, IAuditable
    {
        _context.Entry(entity).OriginalValues[nameof(entity.UpdatedAt)] = originalDateTimeUpdated;
        _context.Entry(entity).CurrentValues[nameof(entity.UpdatedAt)] = newDateTimeUpdated;
        _context.Set<T>().Update(entity);
    }

    public void UpdateWithConcurrency<T>(T entity, DateTimeOffset originalDateTimeUpdated) where T : class, TEntity, IAuditable
    {
        UpdateWithConcurrency(entity, originalDateTimeUpdated, DateTimeOffset.UtcNow);
    }
}