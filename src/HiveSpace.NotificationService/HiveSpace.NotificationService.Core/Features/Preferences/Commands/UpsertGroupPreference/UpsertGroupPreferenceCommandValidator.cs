using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.NotificationService.Core.Features.Preferences.Commands.UpsertGroupPreference;

public class UpsertGroupPreferenceCommandValidator : AbstractValidator<UpsertGroupPreferenceCommand>
{
    public UpsertGroupPreferenceCommandValidator()
    {
        RuleFor(x => x.EventGroup)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(UpsertGroupPreferenceCommand.EventGroup)));
    }
}
