using MediatR;

namespace HiveSpace.OrderService.Application.Cart.Queries.GetCartItems;

public record GetCartItemsQuery(int Page = 1, int PageSize = 20) : IRequest<GetCartItemsResponse>;
