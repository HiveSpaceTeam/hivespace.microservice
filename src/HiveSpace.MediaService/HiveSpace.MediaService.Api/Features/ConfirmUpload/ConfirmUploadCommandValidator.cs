using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.MediaService.Api.Features.ConfirmUpload;

public class ConfirmUploadCommandValidator : AbstractValidator<ConfirmUploadCommand>
{
    public ConfirmUploadCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ConfirmUploadCommand.Id)));

        RuleFor(x => x.EntityId)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ConfirmUploadCommand.EntityId)));
    }
}
