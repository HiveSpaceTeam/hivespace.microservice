namespace HiveSpace.MediaService.Core.Contracts;

public record ConfirmUploadRequest(
    Guid Id,
    string EntityId);
