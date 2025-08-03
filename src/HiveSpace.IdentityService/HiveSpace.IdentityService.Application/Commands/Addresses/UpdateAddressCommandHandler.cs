using HiveSpace.Application.Shared.Handlers;
using HiveSpace.IdentityService.Domain.Repositories;
using HiveSpace.Core.Contexts;
using HiveSpace.IdentityService.Domain.Exceptions;
using HiveSpace.IdentityService.Domain.Aggregates;
using HiveSpace.IdentityService.Application.Models.Responses;

namespace HiveSpace.IdentityService.Application.Commands.Addresses;

public class UpdateAddressCommandHandler : ICommandHandler<UpdateAddressCommand, AddressResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;

    public UpdateAddressCommandHandler(IUserRepository userRepository, IUserContext userContext)
    {
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task<AddressResponseDto> Handle(UpdateAddressCommand command, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId.ToString();
        var user = await _userRepository.GetByIdAsync(userId, includeDetail: true)
            ?? throw new UserNotFoundException();

        var addressProps = new AddressProps
        {
            FullName = command.FullName,
            Street = command.Street,
            Ward = command.Ward,
            District = command.District,
            Province = command.Province,
            Country = command.Country,
            ZipCode = command.ZipCode,
            PhoneNumber = command.PhoneNumber,
        };

        var address = user.UpdateAddress(command.AddressId, addressProps);
        await _userRepository.SaveChangesAsync();
        return new AddressResponseDto(
            address.Id,
            address.FullName,
            address.Street,
            address.Ward,
            address.District,
            address.Province,
            address.Country,
            address.ZipCode,
            address.PhoneNumber,
            address.IsDefault,
            address.CreatedAt,
            address.UpdatedAt
        );
    }
}

