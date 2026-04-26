using HiveSpace.Domain.Shared.Interfaces;
using HiveSpace.OrderService.Domain.Aggregates.Orders;

namespace HiveSpace.OrderService.Domain.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default);
    Task<Order?> GetByOrderCodeAsync(string orderCode, CancellationToken ct = default);
    Task<Order?> GetByIdAndStoreIdAsync(Guid orderId, Guid storeId, CancellationToken ct = default);
}
