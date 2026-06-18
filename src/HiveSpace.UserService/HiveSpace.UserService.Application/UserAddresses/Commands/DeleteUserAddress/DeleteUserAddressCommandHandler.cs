using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;

namespace HiveSpace.UserService.Application.UserAddresses.Commands.DeleteUserAddress;

public class DeleteUserAddressCommandHandler(
    IUserContext userContext,
    IUserRepository userRepository)
    : ICommandHandler<DeleteUserAddressCommand>
{
    public async Task Handle(DeleteUserAddressCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(
                userContext.UserId,
                includeDetail: true,
                cancellationToken: cancellationToken,
                asTracking: true)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        user.RemoveAddress(request.AddressId);
        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
