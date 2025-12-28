using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.MediaService.Api.Features.PresignUrl;

public class PresignUrlCommandValidator : AbstractValidator<PresignUrlCommand>
{
    public PresignUrlCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(PresignUrlCommand.FileName)));

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(PresignUrlCommand.ContentType)));

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(PresignUrlCommand.FileSize)));

        RuleFor(x => x.EntityType)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(PresignUrlCommand.EntityType)));
    }
}
