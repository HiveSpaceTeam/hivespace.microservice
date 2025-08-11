using HiveSpace.OrderService.Domain.Enums;

namespace HiveSpace.OrderService.Application.DTOs;

public record OrderItemResponse(
    Guid Id,
    int SkuId,
    string ProductName,
    string VariantName,
    string Thumbnail,
    int Quantity,
    double Amount,
    Currency Currency
) : OrderItemBaseDto(SkuId, ProductName, VariantName, Thumbnail, Quantity, Amount, Currency);