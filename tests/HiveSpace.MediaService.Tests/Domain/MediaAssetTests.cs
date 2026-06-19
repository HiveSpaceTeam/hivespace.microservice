using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.MediaService.Core.DomainModels;
using Xunit;

namespace HiveSpace.MediaService.Tests.Domain;

public class MediaAssetTests
{
    private static MediaAsset NewAsset() =>
        new("file.jpg", "uploads/file.jpg", "product");

    [Fact]
    public void Create_WithValidFields_StartsInPendingStatus()
    {
        var asset = NewAsset();
        asset.Status.Should().Be(MediaStatus.Pending);
    }

    [Fact]
    public void MarkAsUploaded_FromPendingStatus_TransitionsToUploaded()
    {
        var asset = NewAsset();
        asset.MarkAsUploaded();
        asset.Status.Should().Be(MediaStatus.Uploaded);
    }

    [Fact]
    public void MarkAsUploaded_WhenNotPending_ThrowsDomainException()
    {
        var asset = NewAsset();
        asset.MarkAsUploaded();

        var act = () => asset.MarkAsUploaded();
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkAsProcessed_SetsPublicUrl()
    {
        var asset = NewAsset();
        asset.MarkAsUploaded();
        asset.MarkAsProcessed("https://cdn.example.com/file.jpg");
        asset.PublicUrl.Should().Be("https://cdn.example.com/file.jpg");
        asset.Status.Should().Be(MediaStatus.Processed);
    }

    [Fact]
    public void MarkAsFailed_SetsStatusToFailed()
    {
        var asset = NewAsset();
        asset.MarkAsFailed();
        asset.Status.Should().Be(MediaStatus.Failed);
    }

    [Fact]
    public void SetEntityId_WhenNotPending_ThrowsDomainException()
    {
        var asset = NewAsset();
        asset.MarkAsUploaded();

        var act = () => asset.SetEntityId("entity-123");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void SetEntityId_WithBlankString_ThrowsDomainException()
    {
        var asset = NewAsset();

        var act = () => asset.SetEntityId("  ");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateFileSize_SetsNewSize()
    {
        var asset = NewAsset();
        asset.UpdateFileSize(2048);
        asset.FileSize.Should().Be(2048);
    }

    [Fact]
    public void UpdateStorageDetails_SetsPathAndMimeType()
    {
        var asset = NewAsset();
        asset.UpdateStorageDetails("uploads/renamed.jpg", "image/jpeg");
        asset.StoragePath.Should().Be("uploads/renamed.jpg");
        asset.MimeType.Should().Be("image/jpeg");
    }

    [Fact]
    public void MarkAsProcessed_WithThumbnailUrl_SetsBothUrls()
    {
        var asset = NewAsset();
        asset.MarkAsProcessed("https://cdn.example.com/file.jpg", "https://cdn.example.com/thumb.jpg");
        asset.PublicUrl.Should().Be("https://cdn.example.com/file.jpg");
        asset.ThumbnailUrl.Should().Be("https://cdn.example.com/thumb.jpg");
    }

    [Theory]
    [InlineData(null, "path", "type")]
    [InlineData("file", null, "type")]
    [InlineData("file", "path", null)]
    public void Create_WithNullRequiredField_ThrowsInvalidFieldException(
        string? fileName, string? storagePath, string? entityType)
    {
        var act = () => new MediaAsset(fileName!, storagePath!, entityType!);
        act.Should().Throw<InvalidFieldException>();
    }

    [Fact]
    public void SetEntityId_WithValidString_SetsEntityId()
    {
        var asset = NewAsset();
        asset.SetEntityId("entity-123");
        asset.EntityId.Should().Be("entity-123");
    }

    [Fact]
    public void Create_WithAllOptionalFields_SetsThemCorrectly()
    {
        var asset = new MediaAsset("file.jpg", "uploads/file.jpg", "product",
            originalFileName: "original.jpg",
            mimeType: "image/jpeg",
            fileSize: 1024,
            entityId: "ent-1");

        asset.OriginalFileName.Should().Be("original.jpg");
        asset.MimeType.Should().Be("image/jpeg");
        asset.FileSize.Should().Be(1024);
        asset.EntityId.Should().Be("ent-1");
    }

    [Fact]
    public void EfCoreConstructor_InitializesNullSentinels()
    {
        var asset = new TestableMediaAsset();
        asset.FileName.Should().Be(null!);
    }

    [Fact]
    public void Create_UpdatedAt_IsNullInitially()
    {
        var asset = NewAsset();
        asset.UpdatedAt.Should().BeNull();
    }

    private sealed class TestableMediaAsset : MediaAsset { }
}
