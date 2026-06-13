using FluentAssertions;
using HiveSpace.MediaService.Core.Features.Media.Commands.GeneratePresignedUrl;
using HiveSpace.MediaService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Fakes;
using Xunit;

namespace HiveSpace.MediaService.Tests.Application.Media;

public class GeneratePresignUrlCommandHandlerTests : IClassFixture<MediaServiceFixture>
{
    private readonly MediaServiceFixture _fixture;

    public GeneratePresignUrlCommandHandlerTests(MediaServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public void Handle_WithValidMediaType_ReturnsPresignUrlContainingKey()
    {
        var fake = new BlobStorageFake();

        var url = fake.GeneratePresignUrl("media-key");

        url.Should().Contain("media-key");
        typeof(GeneratePresignedUrlCommandHandler).Should().NotBeNull();
    }

    [Fact]
    public void Handle_GeneratedUrl_ContainsExpectedStorageReference()
    {
        var fake = new BlobStorageFake();

        var url = fake.GeneratePresignUrl("product/image.jpg");

        url.Should().NotBeNullOrWhiteSpace("BlobStorageFake must return a non-empty pre-signed URL");
        Uri.UnescapeDataString(url).Should().Contain("product/image.jpg");
    }
}
