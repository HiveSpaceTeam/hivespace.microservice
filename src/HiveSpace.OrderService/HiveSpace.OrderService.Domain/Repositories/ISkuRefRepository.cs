using HiveSpace.OrderService.Domain.External;

namespace HiveSpace.OrderService.Domain.Repositories;

public interface ISkuRefRepository
{
    Task<bool> ExistsAsync(long skuId, long productId, CancellationToken cancellationToken = default);
    Task<List<SkuRef>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken ct = default);
}
