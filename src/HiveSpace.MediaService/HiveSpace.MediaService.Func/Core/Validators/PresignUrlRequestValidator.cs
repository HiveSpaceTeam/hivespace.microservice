using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.MediaService.Func.Core.Contracts;

namespace HiveSpace.MediaService.Func.Core.Validators;

public class PresignUrlRequestValidator : AbstractValidator<PresignUrlRequest>
{
    public PresignUrlRequestValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(PresignUrlRequest.FileName)));

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(PresignUrlRequest.ContentType)));

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(PresignUrlRequest.FileSize)));

        RuleFor(x => x.EntityType)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(PresignUrlRequest.EntityType)));
    }
}
