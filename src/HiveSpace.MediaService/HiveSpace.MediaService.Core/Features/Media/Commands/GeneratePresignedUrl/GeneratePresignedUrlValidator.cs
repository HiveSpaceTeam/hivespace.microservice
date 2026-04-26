using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.MediaService.Core.Features.Media.Commands.GeneratePresignedUrl;

public class GeneratePresignedUrlValidator : AbstractValidator<GeneratePresignedUrlCommand>
{
    public GeneratePresignedUrlValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(GeneratePresignedUrlCommand.FileName)));

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(GeneratePresignedUrlCommand.ContentType)));

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(GeneratePresignedUrlCommand.FileSize)));

        RuleFor(x => x.EntityType)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(GeneratePresignedUrlCommand.EntityType)));
    }
}
