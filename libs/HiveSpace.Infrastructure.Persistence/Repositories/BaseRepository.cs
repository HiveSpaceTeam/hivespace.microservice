using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.Domain.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HiveSpace.Infrastructure.Persistence.Repositories;

public abstract class BaseRepository<TEntity, TKey>(DbContext context) : IRepository<TEntity>
    where TEntity : class, IAggregateRoot
    where TKey : struct, IEquatable<TKey>
{
    protected readonly DbContext _context = context;
    protected readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public void Add(TEntity entity)
    {
        _dbSet.Add(entity);
    }

    public async Task<List<TEntity>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    protected virtual IQueryable<TEntity> ApplyIncludeDetail(IQueryable<TEntity> query)
    {
        return query; // Default implementation returns query without includes
    }

    public async Task<TEntity?> GetByIdAsync(object id, bool includeDetail = false)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        if (includeDetail)
        {
            query = ApplyIncludeDetail(query);
        }

        return await query.FirstOrDefaultAsync(entity => EF.Property<object>(entity, "Id") == id);
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