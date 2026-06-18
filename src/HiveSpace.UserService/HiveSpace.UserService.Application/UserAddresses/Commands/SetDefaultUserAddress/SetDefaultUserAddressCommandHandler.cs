using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;

namespace HiveSpace.UserService.Application.UserAddresses.Commands.SetDefaultUserAddress;

public class SetDefaultUserAddressCommandHandler(
    IUserContext userContext,
    IUserRepository userRepository)
    : ICommandHandler<SetDefaultUserAddressCommand>
{
    public async Task Handle(SetDefaultUserAddressCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(
                userContext.UserId,
                includeDetail: true,
                cancellationToken: cancellationToken,
                asTracking: true)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        user.MarkAddressAsDefault(request.AddressId);
        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
