using HiveSpace.OrderService.Domain.Enums;

namespace HiveSpace.OrderService.Application.DTOs;

public record OrderItemBaseDto(
    int SkuId,
    string ProductName,
    string VariantName,
    string Thumbnail,
    int Quantity,
    double Amount,
    Currency Currency
);