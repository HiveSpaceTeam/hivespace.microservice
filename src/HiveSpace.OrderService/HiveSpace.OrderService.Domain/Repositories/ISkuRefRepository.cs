namespace HiveSpace.OrderService.Domain.Repositories;

public interface ISkuRefRepository
{
    Task<bool> ExistsAsync(long skuId, long productId, CancellationToken cancellationToken = default);
}
