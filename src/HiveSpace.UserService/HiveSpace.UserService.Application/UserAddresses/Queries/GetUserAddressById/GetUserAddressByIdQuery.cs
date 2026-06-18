using MediatR;
using HiveSpace.UserService.Application.UserAddresses.Dtos;

namespace HiveSpace.UserService.Application.UserAddresses.Queries.GetUserAddressById;

public record GetUserAddressByIdQuery(Guid AddressId) : IRequest<UserAddressDto?>;
