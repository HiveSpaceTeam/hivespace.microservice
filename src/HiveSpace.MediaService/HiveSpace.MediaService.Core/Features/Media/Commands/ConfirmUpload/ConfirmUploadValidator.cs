using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.MediaService.Core.Features.Media.Commands.ConfirmUpload;

public class ConfirmUploadValidator : AbstractValidator<ConfirmUploadCommand>
{
    public ConfirmUploadValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ConfirmUploadCommand.FileId)));

        RuleFor(x => x.EntityId)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ConfirmUploadCommand.EntityId)));
    }
}
