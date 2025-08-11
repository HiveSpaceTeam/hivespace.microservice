using HiveSpace.OrderService.Application.Commands;
using HiveSpace.OrderService.Application.DTOs;

namespace HiveSpace.OrderService.Application.Services;

public interface IOrderService
{
    Task<Guid> CreateOrderAsync(CreateOrderRequest request);
    Task<List<OrderResponse>> GetOrdersAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters = null);
    Task<OrderResponse?> GetOrderByIdAsync(Guid orderId);
    Task<bool> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request);
}