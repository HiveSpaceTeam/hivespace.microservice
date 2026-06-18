using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Application.Users.Dtos;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Application.Users.Commands.UpdateUserSetting;

public class UpdateUserSettingCommandValidator : AbstractValidator<UpdateUserSettingCommand>
{
    public UpdateUserSettingCommandValidator()
    {
        RuleFor(x => x.Payload)
            .Must(p => p.Theme is not null || p.Culture is not null)
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UserSettings)));

        When(x => x.Payload.Theme is not null, () =>
        {
            RuleFor(x => x.Payload.Theme!)
                .Must(value => UserSettingValues.SupportedThemes.Contains(value))
                .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(UpdateUserSettingRequestDto.Theme)));
        });

        When(x => x.Payload.Culture is not null, () =>
        {
            RuleFor(x => x.Payload.Culture!)
                .Must(value => UserSettingValues.SupportedCultures.Contains(value))
                .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(UpdateUserSettingRequestDto.Culture)));
        });
    }
}
