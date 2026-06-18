using FluentAssertions;
using HiveSpace.MediaService.Core.DomainModels;
using HiveSpace.MediaService.Core.Features.Media.Commands.GeneratePresignedUrl;
using HiveSpace.MediaService.Core.Infrastructure.Configuration;
using HiveSpace.MediaService.Core.Interfaces;
using HiveSpace.MediaService.Core.Persistence.Repositories;
using HiveSpace.MediaService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace HiveSpace.MediaService.Tests.Application.Presign;

public class GeneratePresignUrlCommandHandlerTests : IClassFixture<MediaServiceFixture>
{
    private readonly MediaServiceFixture _fixture;

    public GeneratePresignUrlCommandHandlerTests(MediaServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithValidMediaType_ReturnsPresignUrlAndUploadRef()
    {
        var storageService = new StorageServiceFake();
        var repository = new MediaAssetRepository(_fixture.DbContext);
        var handler = new GeneratePresignedUrlCommandHandler(
            repository,
            storageService,
            new StorageConfiguration(new ConfigurationBuilder().Build()));

        var result = await handler.Handle(
            new GeneratePresignedUrlCommand("image.png", "image/png", 512, "product", "product-1"),
            CancellationToken.None);

        result.UploadUrl.Should().Contain("temp-media-upload");
        result.StoragePath.Should().StartWith("product/");
        storageService.EnsuredContainers.Should().Contain("temp-media-upload");
    }

    [Fact]
    public async Task Handle_AfterPresign_AssetStartsInPendingStatus()
    {
        var storageService = new StorageServiceFake();
        var repository = new MediaAssetRepository(_fixture.DbContext);
        var handler = new GeneratePresignedUrlCommandHandler(
            repository,
            storageService,
            new StorageConfiguration(new ConfigurationBuilder().Build()));

        var result = await handler.Handle(
            new GeneratePresignedUrlCommand("video.mp4", "video/mp4", 2048, "product", "product-2"),
            CancellationToken.None);

        var stored = await _fixture.DbContext.MediaAssets.SingleAsync(a => a.Id == result.FileId);
        stored.Status.Should().Be(MediaStatus.Pending, "generated presign URLs are created for Pending media assets");
    }

    private sealed class StorageServiceFake : IStorageService
    {
        private readonly List<string> _ensuredContainers = [];

        public IReadOnlyList<string> EnsuredContainers => _ensuredContainers;

        public Uri GeneratePresignedUrl(string containerName, string blobName, StoragePermissions permissions, int expiryMinutes)
            => new($"https://blob.hivespace.test/{containerName}/{Uri.EscapeDataString(blobName)}");

        public string GetContainerUrl(string containerName) => $"https://blob.hivespace.test/{containerName}";

        public Task<Stream> DownloadBlobAsync(string containerName, string blobName) => throw new NotSupportedException();

        public Task UploadBlobAsync(string containerName, string blobName, Stream content, string contentType) => throw new NotSupportedException();

        public Task DeleteBlobAsync(string containerName, string blobName) => throw new NotSupportedException();

        public Task ConfigureCorsAsync(string[] allowedOrigins, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task EnsureContainerExistsAsync(string containerName)
        {
            _ensuredContainers.Add(containerName);
            return Task.CompletedTask;
        }

        public string GetPublicUrl(string containerName, string blobName, string? cdnHost = null)
            => $"https://blob.hivespace.test/{containerName}/{Uri.EscapeDataString(blobName)}";
    }
}
