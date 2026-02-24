namespace HiveSpace.MediaService.Core.Contracts;

public record PresignUrlResponse(
    Guid FileId,
    string UploadUrl,
    string StoragePath,
    DateTimeOffset ExpiresAt);
