using HiveSpace.OrderService.Domain.External;

namespace HiveSpace.OrderService.Domain.Repositories;

public interface IProductRefRepository
{
    Task<List<ProductRef>> GetByIdsAsync(IEnumerable<long> ids, CancellationToken ct = default);
}
