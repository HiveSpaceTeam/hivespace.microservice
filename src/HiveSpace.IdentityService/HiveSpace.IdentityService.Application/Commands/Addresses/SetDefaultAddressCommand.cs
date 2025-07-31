using HiveSpace.Application.Shared.Commands;

namespace HiveSpace.IdentityService.Application.Commands.Addresses;

public record SetDefaultAddressCommand(Guid AddressId) : ICommand;
