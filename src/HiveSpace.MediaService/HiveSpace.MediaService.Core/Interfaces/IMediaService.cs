using HiveSpace.MediaService.Core.Contracts;

namespace HiveSpace.MediaService.Core.Interfaces;

public interface IMediaService
{
    Task<PresignUrlResponse> GeneratePresignedUrlAsync(PresignUrlRequest request);
    Task ConfirmUploadAsync(ConfirmUploadRequest request);
}
