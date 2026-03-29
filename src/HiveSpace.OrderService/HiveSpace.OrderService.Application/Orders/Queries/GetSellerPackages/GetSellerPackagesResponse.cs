using HiveSpace.OrderService.Application.Orders.Dtos;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetSellerPackages;

public record GetSellerPackagesResponse(
    List<PackageSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
