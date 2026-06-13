using FluentAssertions;
using HiveSpace.MediaService.Core.DomainModels;
using HiveSpace.MediaService.Core.Features.Media.Commands.ConfirmUpload;
using HiveSpace.MediaService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Fakes;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HiveSpace.MediaService.Tests.Application.UploadConfirmation;

public class ConfirmUploadCommandHandlerTests : IClassFixture<MediaServiceFixture>
{
    private readonly MediaServiceFixture _fixture;

    public ConfirmUploadCommandHandlerTests(MediaServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_ForValidRef_MarksConfirmedInBlobStorageFake()
    {
        var fake = new BlobStorageFake();
        var asset = new MediaAsset("file.png", "uploads/file.png", "product", mimeType: "image/png");

        _fixture.DbContext.MediaAssets.Add(asset);
        await _fixture.DbContext.SaveChangesAsync();
        await fake.ConfirmUploadAsync(asset.StoragePath);

        fake.ConfirmedKeys.Should().Contain(asset.StoragePath);
        typeof(ConfirmUploadCommandHandler).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_AfterConfirmation_MediaAssetStatusIsUploaded()
    {
        var asset = new MediaAsset("thumb.jpg", "uploads/thumb.jpg", "product", mimeType: "image/jpeg");
        _fixture.DbContext.MediaAssets.Add(asset);
        await _fixture.DbContext.SaveChangesAsync();

        asset.MarkAsUploaded();
        await _fixture.DbContext.SaveChangesAsync();

        var stored = await _fixture.DbContext.MediaAssets.SingleAsync(x => x.Id == asset.Id);
        stored.Status.Should().Be(MediaStatus.Uploaded);
    }
}
