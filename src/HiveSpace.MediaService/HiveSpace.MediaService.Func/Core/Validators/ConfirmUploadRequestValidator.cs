using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.MediaService.Func.Core.Contracts;

namespace HiveSpace.MediaService.Func.Core.Validators;

public class ConfirmUploadRequestValidator : AbstractValidator<ConfirmUploadRequest>
{
    public ConfirmUploadRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ConfirmUploadRequest.Id)));

        RuleFor(x => x.EntityId)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(ConfirmUploadRequest.EntityId)));
    }
}
