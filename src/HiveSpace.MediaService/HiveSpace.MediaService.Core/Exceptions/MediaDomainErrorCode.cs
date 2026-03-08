using HiveSpace.Domain.Shared.Errors;

namespace HiveSpace.MediaService.Core.Exceptions;

public class MediaDomainErrorCode : DomainErrorCode
{
    private MediaDomainErrorCode(int id, string name, string code) : base(id, name, code) { }

    public static readonly MediaDomainErrorCode MediaNotFound = new(1001, "MediaNotFound", "MED001");
    public static readonly MediaDomainErrorCode MediaUploadFailed = new(1002, "MediaUploadFailed", "MED002");
    public static readonly MediaDomainErrorCode MediaProcessingFailed = new(1003, "MediaProcessingFailed", "MED003");
    public static readonly MediaDomainErrorCode StorageConfigurationMissing = new(1004, "StorageConfigurationMissing", "MED004");
    public static readonly MediaDomainErrorCode BlobNotFound = new(1005, "BlobNotFound", "MED005");
}
