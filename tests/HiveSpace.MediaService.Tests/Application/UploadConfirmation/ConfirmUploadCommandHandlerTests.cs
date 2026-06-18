using FluentAssertions;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.MediaService.Core.DomainModels;
using HiveSpace.MediaService.Core.Features.Media.Commands.ConfirmUpload;
using HiveSpace.MediaService.Core.Interfaces;
using HiveSpace.MediaService.Core.Persistence.Repositories;
using HiveSpace.MediaService.Tests.Fixtures;
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
        var asset = new MediaAsset("file.png", "uploads/file.png", "product", mimeType: "image/png");
        var queue = new QueueServiceFake();
        var repository = new MediaAssetRepository(_fixture.DbContext);
        var handler = new ConfirmUploadCommandHandler(repository, queue);

        _fixture.DbContext.MediaAssets.Add(asset);
        await _fixture.DbContext.SaveChangesAsync();
        await handler.Handle(new ConfirmUploadCommand(asset.Id, "product-1"), CancellationToken.None);

        queue.Messages.Should().ContainSingle();
        queue.Messages[0].Should().Contain(asset.Id.ToString());
    }

    [Fact]
    public async Task Handle_AfterConfirmation_MediaAssetStatusIsUploaded()
    {
        var asset = new MediaAsset("thumb.jpg", "uploads/thumb.jpg", "product", mimeType: "image/jpeg");
        var queue = new QueueServiceFake();
        var repository = new MediaAssetRepository(_fixture.DbContext);
        var handler = new ConfirmUploadCommandHandler(repository, queue);
        _fixture.DbContext.MediaAssets.Add(asset);
        await _fixture.DbContext.SaveChangesAsync();

        await handler.Handle(new ConfirmUploadCommand(asset.Id, "product-2"), CancellationToken.None);

        var stored = await _fixture.DbContext.MediaAssets.SingleAsync(x => x.Id == asset.Id);
        stored.Status.Should().Be(MediaStatus.Uploaded);
        stored.EntityId.Should().Be("product-2");
    }

    [Fact]
    public async Task Handle_WithUnknownFileId_ThrowsNotFoundException()
    {
        var queue = new QueueServiceFake();
        var repository = new MediaAssetRepository(_fixture.DbContext);
        var handler = new ConfirmUploadCommandHandler(repository, queue);

        var act = async () => await handler.Handle(new ConfirmUploadCommand(Guid.NewGuid(), "product-x"), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    private sealed class QueueServiceFake : IQueueService
    {
        private readonly List<string> _messages = [];

        public IReadOnlyList<string> Messages => _messages;

        public Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            _messages.Add(message);
            return Task.CompletedTask;
        }
    }
}
