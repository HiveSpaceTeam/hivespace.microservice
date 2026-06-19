using HiveSpace.Application.Shared.Commands;
using HiveSpace.UserService.Application.UserAddresses.Dtos;

namespace HiveSpace.UserService.Application.UserAddresses.Commands.CreateUserAddress;

public record CreateUserAddressCommand(UserAddressRequestDto Payload) : ICommand<UserAddressDto>;
