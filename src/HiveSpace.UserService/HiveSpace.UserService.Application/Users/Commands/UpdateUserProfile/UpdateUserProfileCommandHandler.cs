using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Application.Users.Dtos;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;

namespace HiveSpace.UserService.Application.Users.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandHandler(
    IUserContext userContext,
    IUserRepository userRepository)
    : ICommandHandler<UpdateUserProfileCommand>
{
    public async Task Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var payload = request.Payload;
        var user = await userRepository.GetByIdAsync(
                userContext.UserId,
                includeDetail: true,
                cancellationToken: cancellationToken,
                asTracking: true)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        if (payload.UserName != null)
        {
            // Non-atomic check: unique index on UserName column is the hard enforcement.
            var existing = await userRepository.GetByUserNameAsync(payload.UserName, cancellationToken);
            if (existing is not null && existing.Id != user.Id)
                throw new ConflictException(UserDomainErrorCode.UserNameAlreadyExists, nameof(User.UserName));
        }

        var phoneNumber = payload.PhoneNumber != null ? PhoneNumber.Create(payload.PhoneNumber) : null;
        var dateOfBirth = payload.DateOfBirth.HasValue ? DateOfBirth.Create(payload.DateOfBirth.Value) : null;

        user.UpdateProfile(payload.FullName, phoneNumber, dateOfBirth, payload.Gender, payload.UserName);
        if (payload.AvatarFileId != null)
            user.SetAvatar(payload.AvatarFileId);

        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
