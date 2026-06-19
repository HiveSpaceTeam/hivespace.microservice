using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Application.Users.Dtos;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;

namespace HiveSpace.UserService.Application.Users.Queries.GetUserProfile;

public class GetUserProfileQueryHandler(
    IUserContext userContext,
    IUserRepository userRepository)
    : IQueryHandler<GetUserProfileQuery, GetUserProfileResponseDto>
{
    public async Task<GetUserProfileResponseDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userContext.UserId, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        return new GetUserProfileResponseDto(
            user.FullName,
            user.UserName,
            user.Email.Value,
            user.AvatarUrl,
            user.PhoneNumber?.Value,
            user.Gender,
            user.DateOfBirth?.Value
        );
    }
}
