using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.UserService.Application.UserAddresses.Commands.DeleteUserAddress;

public record DeleteUserAddressCommand(Guid AddressId) : ICommand;
