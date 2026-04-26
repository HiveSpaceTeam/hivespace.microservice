namespace HiveSpace.MediaService.Core.Features.Media.Dtos;

public record PresignUrlResponse(
    Guid FileId,
    string UploadUrl,
    string StoragePath,
    DateTimeOffset ExpiresAt);
