using MediatR;
using HiveSpace.UserService.Application.UserAddresses.Dtos;

namespace HiveSpace.UserService.Application.UserAddresses.Queries.GetDefaultUserAddress;

public record GetDefaultUserAddressQuery : IRequest<UserAddressDto?>;
