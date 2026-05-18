using FluentValidation;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.MediaService.Core.Features.Media.Commands.GeneratePresignedUrl;

public class GeneratePresignedUrlValidator : AbstractValidator<GeneratePresignedUrlCommand>
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> ImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

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

        RuleFor(x => x.ContentType)
            .Must(contentType => ImageContentTypes.Contains(contentType))
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(GeneratePresignedUrlCommand.ContentType)));

        RuleFor(x => x.FileSize)
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithState(_ => new Error(CommonErrorCode.InvalidArgument, nameof(GeneratePresignedUrlCommand.FileSize)));
    }
}
