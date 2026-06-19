using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Application.Users.Dtos;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;

namespace HiveSpace.UserService.Application.Users.Queries.GetUserSetting;

public class GetUserSettingQueryHandler(
    IUserContext userContext,
    IUserRepository userRepository)
    : IQueryHandler<GetUserSettingQuery, GetUserSettingsResponseDto>
{
    public async Task<GetUserSettingsResponseDto> Handle(GetUserSettingQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userContext.UserId, cancellationToken: cancellationToken)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        return new GetUserSettingsResponseDto(
            UserSettingValues.ToApiValue(user.Settings.Theme),
            UserSettingValues.ToApiValue(user.Settings.Culture)
        );
    }
}
