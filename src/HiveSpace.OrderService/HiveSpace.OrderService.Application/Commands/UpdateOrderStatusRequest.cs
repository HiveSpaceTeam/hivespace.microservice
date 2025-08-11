using HiveSpace.OrderService.Domain.Enums;

namespace HiveSpace.OrderService.Application.Commands;

/// <summary>
/// Request DTO for updating order status - belongs to Commands as it represents an action
/// </summary>
public record UpdateOrderStatusRequest(OrderStatus Status);