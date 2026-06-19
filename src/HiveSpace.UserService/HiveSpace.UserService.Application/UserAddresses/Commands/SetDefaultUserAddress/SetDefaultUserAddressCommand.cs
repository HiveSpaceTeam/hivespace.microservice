using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.UserService.Application.UserAddresses.Commands.SetDefaultUserAddress;

public record SetDefaultUserAddressCommand(Guid AddressId) : ICommand;
