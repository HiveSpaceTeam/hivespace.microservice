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
}
