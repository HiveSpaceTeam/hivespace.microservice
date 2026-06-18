using HiveSpace.Application.Shared.Handlers;
using HiveSpace.Core.Contexts;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.UserService.Application.Users.Dtos;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Exceptions;
using HiveSpace.UserService.Domain.Repositories;

namespace HiveSpace.UserService.Application.Users.Commands.UpdateUserSetting;

public class UpdateUserSettingCommandHandler(
    IUserContext userContext,
    IUserRepository userRepository)
    : ICommandHandler<UpdateUserSettingCommand>
{
    public async Task Handle(UpdateUserSettingCommand request, CancellationToken cancellationToken)
    {
        var payload = request.Payload;
        var user = await userRepository.GetByIdAsync(
                userContext.UserId,
                includeDetail: true,
                cancellationToken: cancellationToken,
                asTracking: true)
            ?? throw new NotFoundException(UserDomainErrorCode.UserNotFound, nameof(User));

        if (payload.Theme is not null)
            user.UpdateTheme(UserSettingValues.ToTheme(payload.Theme));

        if (payload.Culture is not null)
            user.UpdateCulture(UserSettingValues.ToCulture(payload.Culture));

        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
