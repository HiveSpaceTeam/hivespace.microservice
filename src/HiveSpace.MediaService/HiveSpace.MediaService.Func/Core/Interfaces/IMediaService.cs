using HiveSpace.MediaService.Func.Core.Contracts;

namespace HiveSpace.MediaService.Func.Core.Interfaces;

public interface IMediaService
{
    Task<PresignUrlResponse> GeneratePresignedUrlAsync(PresignUrlRequest request);
    Task ConfirmUploadAsync(ConfirmUploadRequest request);
}
