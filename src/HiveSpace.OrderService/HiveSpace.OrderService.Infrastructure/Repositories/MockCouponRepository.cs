using System.Collections.Concurrent;
using HiveSpace.Domain.Shared.Specifications;
using HiveSpace.OrderService.Domain.Aggregates.Coupons;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.OrderService.Infrastructure.Repositories;

public class MockCouponRepository : ICouponRepository
{
    private static readonly ConcurrentDictionary<Guid, Coupon> _coupons = new();

    public IQueryable<Coupon> GetQueryable()
    {
        return _coupons.Values.AsQueryable();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(1);
    }

    public void Add(Coupon entity)
    {
        _coupons.TryAdd(entity.Id, entity);
    }

    public void Remove(Coupon entity)
    {
        _coupons.TryRemove(entity.Id, out _);
    }

    public Task<List<Coupon>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_coupons.Values.ToList());
    }

    public Task<List<Coupon>> GetListAsync(Specification<Coupon> specification, CancellationToken cancellationToken = default)
    {
        var filtered = _coupons.Values
            .Where(specification.ToExpression().Compile())
            .ToList();
        return Task.FromResult(filtered);
    }

    public Task<int> GetCountAsync(Specification<Coupon> specification, CancellationToken cancellationToken = default)
    {
        var count = _coupons.Values
            .Where(specification.ToExpression().Compile())
            .Count();
        return Task.FromResult(count);
    }

    public Task<List<Coupon>> GetPagedAsync(Specification<Coupon> specification, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var paged = _coupons.Values
            .Where(specification.ToExpression().Compile())
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Task.FromResult(paged);
    }

    public Task<Coupon?> GetByIdAsync(object id, bool includeDetail = false, CancellationToken cancellationToken = default)
    {
        if (id is Guid guidId && _coupons.TryGetValue(guidId, out var coupon))
        {
            return Task.FromResult<Coupon?>(coupon);
        }
        
        if (id is string stringId && Guid.TryParse(stringId, out var parsedGuid) && _coupons.TryGetValue(parsedGuid, out var couponStr))
        {
            return Task.FromResult<Coupon?>(couponStr);
        }

        return Task.FromResult<Coupon?>(null);
    }

    void IRepository<Coupon>.UpdateWithConcurrency<T>(T entity, DateTimeOffset originalDateTimeUpdated, DateTimeOffset newDateTimeUpdated)
    {
        _coupons.AddOrUpdate(entity.Id, entity, (_, _) => entity);
    }

    void IRepository<Coupon>.UpdateWithConcurrency<T>(T entity, DateTimeOffset originalDateTimeUpdated)
    {
        _coupons.AddOrUpdate(entity.Id, entity, (_, _) => entity);
    }
}

