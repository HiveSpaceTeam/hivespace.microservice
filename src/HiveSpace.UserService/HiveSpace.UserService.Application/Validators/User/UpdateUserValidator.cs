using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Application.Models.Requests.User;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;

namespace HiveSpace.UserService.Application.Validators.User;

public class UpdateUserSettingValidator : AbstractValidator<UpdateUserSettingRequestDto>
{
    public UpdateUserSettingValidator()
    {
        RuleFor(x => x)
            .Must(x => x.Theme.HasValue || x.Culture.HasValue)
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UserSettings)));

        When(x => x.Theme.HasValue, () =>
        {
            RuleFor(x => x.Theme!.Value)
                .IsInEnum()
                .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(Theme)));
        });

        When(x => x.Culture.HasValue, () =>
        {
            RuleFor(x => x.Culture!.Value)
                .IsInEnum()
                .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(Culture)));
        });
    }
}