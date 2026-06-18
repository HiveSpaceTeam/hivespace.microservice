using HiveSpace.Application.Shared.Queries;
using HiveSpace.UserService.Application.UserAddresses.Dtos;

namespace HiveSpace.UserService.Application.UserAddresses.Queries.GetUserAddresses;

public record GetUserAddressesQuery : IQuery<List<UserAddressDto>>;
