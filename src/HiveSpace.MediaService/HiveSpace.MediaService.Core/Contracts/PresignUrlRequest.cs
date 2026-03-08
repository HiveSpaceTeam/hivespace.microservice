namespace HiveSpace.MediaService.Core.Contracts;

public record PresignUrlRequest(
    string FileName,
    string ContentType,
    long FileSize,
    string EntityType,
    string? EntityId);
