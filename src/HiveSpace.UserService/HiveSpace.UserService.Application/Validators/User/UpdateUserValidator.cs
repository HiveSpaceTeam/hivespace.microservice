using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Application.DTOs.User;
using HiveSpace.UserService.Domain.Aggregates.User;

namespace HiveSpace.UserService.Application.Validators.User;

public class UpdateUserSettingValidator : AbstractValidator<UpdateUserSettingRequestDto>
{
    public UpdateUserSettingValidator()
    {
        RuleFor(x => x)
            .Must(x => x.Theme is not null || x.Culture is not null)
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UserSettings)));

        When(x => x.Theme is not null, () =>
        {
            RuleFor(x => x.Theme!)
                .Must(value => UserSettingValues.SupportedThemes.Contains(value))
                .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(UpdateUserSettingRequestDto.Theme)));
        });

        When(x => x.Culture is not null, () =>
        {
            RuleFor(x => x.Culture!)
                .Must(value => UserSettingValues.SupportedCultures.Contains(value))
                .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(UpdateUserSettingRequestDto.Culture)));
        });
    }
}
