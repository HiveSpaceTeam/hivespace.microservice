using HiveSpace.Application.Shared.Commands;
using HiveSpace.UserService.Application.UserAddresses.Dtos;

namespace HiveSpace.UserService.Application.UserAddresses.Commands.UpdateUserAddress;

public record UpdateUserAddressCommand(Guid AddressId, UserAddressRequestDto Payload) : ICommand;
