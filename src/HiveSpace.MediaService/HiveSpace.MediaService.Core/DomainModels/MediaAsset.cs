using HiveSpace.Domain.Shared.Entities;
using HiveSpace.Domain.Shared.Interfaces;

namespace HiveSpace.MediaService.Core.DomainModels;


public class MediaAsset : AggregateRoot<Guid>, IAuditable
{
    public string FileName { get; private set; }
    public string? OriginalFileName { get; private set; }
    public string StoragePath { get; private set; }
    public string? PublicUrl { get; private set; }
    public string? ThumbnailUrl { get; private set; }
    public string? MimeType { get; private set; }
    public long? FileSize { get; private set; }
    public string EntityType { get; private set; }
    public string? EntityId { get; private set; }
    public MediaStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Constructor for EF Core
    protected MediaAsset() { }

    public MediaAsset(
        string fileName,
        string storagePath,
        string entityType,
        string? originalFileName = null,
        string? mimeType = null,
        long? fileSize = null,
        string? entityId = null)
    {
        Id = Guid.NewGuid();
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        StoragePath = storagePath ?? throw new ArgumentNullException(nameof(storagePath));
        EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
        OriginalFileName = originalFileName;
        MimeType = mimeType;
        FileSize = fileSize;
        EntityId = entityId;
        Status = MediaStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsUploaded()
    {
        Status = MediaStatus.Uploaded;
    }

    public void MarkAsProcessed(string? publicUrl, string? thumbnailUrl = null)
    {
        PublicUrl = publicUrl;
        ThumbnailUrl = thumbnailUrl;
        Status = MediaStatus.Processed;
    }

    public void MarkAsFailed()
    {
        Status = MediaStatus.Failed;
    }
    public void UpdateFileSize(long newSize)
    {
        FileSize = newSize;
    }

    public void UpdateStorageDetails(string newStoragePath, string newMimeType)
    {
        StoragePath = newStoragePath;
        MimeType = newMimeType;
    }

    public void SetEntityId(string? entityId)
    {
        EntityId = entityId;
    }
}
