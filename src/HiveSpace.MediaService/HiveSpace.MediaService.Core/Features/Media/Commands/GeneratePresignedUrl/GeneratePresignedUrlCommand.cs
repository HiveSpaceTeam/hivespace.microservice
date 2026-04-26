using HiveSpace.Application.Shared.Commands;
using HiveSpace.MediaService.Core.Features.Media.Dtos;

namespace HiveSpace.MediaService.Core.Features.Media.Commands.GeneratePresignedUrl;

public record GeneratePresignedUrlCommand(
    string FileName,
    string ContentType,
    long FileSize,
    string EntityType,
    string? EntityId) : ICommand<PresignUrlResponse>;
