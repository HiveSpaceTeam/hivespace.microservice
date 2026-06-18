using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;

namespace HiveSpace.UserService.Application.UserAddresses.Commands.UpdateUserAddress;

public class UpdateUserAddressCommandHandler(
    IUserContext userContext,
    IUserRepository userRepository)
    : ICommandHandler<UpdateUserAddressCommand>
{
    public async Task Handle(UpdateUserAddressCommand request, CancellationToken cancellationToken)
    {
        var payload = request.Payload;
        var user = await userRepository.GetByIdAsync(
                userContext.UserId,
                includeDetail: true,
                cancellationToken: cancellationToken,
                asTracking: true)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        user.UpdateAddress(
            request.AddressId,
            payload.FullName,
            payload.PhoneNumber,
            payload.Street,
            payload.Commune,
            payload.Province,
            payload.Country,
            payload.ZipCode,
            payload.AddressType
        );

        if (payload.IsDefault)
            user.MarkAddressAsDefault(request.AddressId);

        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
