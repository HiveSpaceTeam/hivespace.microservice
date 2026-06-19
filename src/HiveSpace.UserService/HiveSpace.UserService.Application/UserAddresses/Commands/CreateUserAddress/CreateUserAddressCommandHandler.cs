using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Application.UserAddresses.Dtos;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;

namespace HiveSpace.UserService.Application.UserAddresses.Commands.CreateUserAddress;

public class CreateUserAddressCommandHandler(
    IUserContext userContext,
    IUserRepository userRepository)
    : ICommandHandler<CreateUserAddressCommand, UserAddressDto>
{
    public async Task<UserAddressDto> Handle(CreateUserAddressCommand request, CancellationToken cancellationToken)
    {
        var payload = request.Payload;
        var user = await userRepository.GetByIdAsync(
                userContext.UserId,
                includeDetail: true,
                cancellationToken: cancellationToken,
                asTracking: true)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        var createdAddress = user.AddAddress(
            payload.FullName,
            payload.PhoneNumber,
            payload.Street,
            payload.Commune,
            payload.Province,
            payload.Country,
            payload.ZipCode,
            payload.AddressType,
            payload.IsDefault
        );

        await userRepository.SaveChangesAsync(cancellationToken);

        return UserAddressDto.FromAddress(createdAddress);
    }
}
