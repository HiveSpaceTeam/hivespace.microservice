namespace HiveSpace.MediaService.Api.Features.PresignUrl;

public record PresignUrlResponse(
    Guid FileId, 
    string UploadUrl, 
    string StoragePath, 
    DateTime ExpiresAt);
