namespace HiveSpace.MediaService.Func.Core.Contracts;

public record PresignUrlResponse(
    Guid FileId, 
    string UploadUrl, 
    string StoragePath, 
    DateTime ExpiresAt);
