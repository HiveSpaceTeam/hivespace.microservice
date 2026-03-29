using MediatR;

namespace HiveSpace.OrderService.Application.Orders.Queries.GetSellerPackages;

public record GetSellerPackagesQuery(int Page = 1, int PageSize = 20, string? Status = null) : IRequest<GetSellerPackagesResponse>;
