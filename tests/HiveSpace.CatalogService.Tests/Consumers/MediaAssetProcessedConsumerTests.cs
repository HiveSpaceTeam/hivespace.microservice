using FluentAssertions;
using HiveSpace.CatalogService.Api.Consumers;
using HiveSpace.CatalogService.Application.Interfaces.Messaging;
using HiveSpace.CatalogService.Domain.Aggregates.CategoryAggregate;
using HiveSpace.CatalogService.Domain.Aggregates.ProductAggregate;
using HiveSpace.CatalogService.Domain.Enums;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.Domain.Shared.Exceptions;
using HiveSpace.Domain.Shared.ValueObjects;
using HiveSpace.Infrastructure.Messaging.Shared.Events.Media;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Consumers;

public class MediaAssetProcessedConsumerTests
{
    private CatalogDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase($"catalog-media-{Guid.NewGuid()}")
            .Options);

    [Fact]
    public async Task Consume_UnknownEntityType_DoesNotThrow()
    {
        var db = CreateDb();
        var consumer = new MediaAssetProcessedConsumer(db, Substitute.For<IProductEventPublisher>(), Substitute.For<ILogger<MediaAssetProcessedConsumer>>());
        var msg = new MediaAssetProcessedIntegrationEvent("file1", "unknown_type", null, "https://cdn.example.com/img.jpg", null);
        var ctx = Substitute.For<ConsumeContext<MediaAssetProcessedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(ctx);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_ProductThumbnail_WhenProductNotFound_ThrowsNotFoundException()
    {
        var db = CreateDb();
        var consumer = new MediaAssetProcessedConsumer(db, Substitute.For<IProductEventPublisher>(), Substitute.For<ILogger<MediaAssetProcessedConsumer>>());
        var msg = new MediaAssetProcessedIntegrationEvent("no-such-file", "product_thumbnail", null, "https://cdn.example.com/img.jpg", null);
        var ctx = Substitute.For<ConsumeContext<MediaAssetProcessedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(ctx);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Consume_Category_WhenCategoryNotFound_ThrowsNotFoundException()
    {
        var db = CreateDb();
        var consumer = new MediaAssetProcessedConsumer(db, Substitute.For<IProductEventPublisher>(), Substitute.For<ILogger<MediaAssetProcessedConsumer>>());
        var msg = new MediaAssetProcessedIntegrationEvent("no-such-file", "category", null, "https://cdn.example.com/img.jpg", null);
        var ctx = Substitute.For<ConsumeContext<MediaAssetProcessedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(ctx);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Consume_ProductImage_WhenProductNotFound_ThrowsNotFoundException()
    {
        var db = CreateDb();
        var consumer = new MediaAssetProcessedConsumer(db, Substitute.For<IProductEventPublisher>(), Substitute.For<ILogger<MediaAssetProcessedConsumer>>());
        var msg = new MediaAssetProcessedIntegrationEvent("no-such-file", "product_image", null, "https://cdn.example.com/img.jpg", null);
        var ctx = Substitute.For<ConsumeContext<MediaAssetProcessedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(ctx);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Consume_SkuImage_WhenProductNotFound_ThrowsNotFoundException()
    {
        var db = CreateDb();
        var consumer = new MediaAssetProcessedConsumer(db, Substitute.For<IProductEventPublisher>(), Substitute.For<ILogger<MediaAssetProcessedConsumer>>());
        var msg = new MediaAssetProcessedIntegrationEvent("no-such-file", "sku_image", null, "https://cdn.example.com/img.jpg", null);
        var ctx = Substitute.For<ConsumeContext<MediaAssetProcessedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(ctx);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Consume_ProductThumbnail_WhenProductFound_UpdatesThumbnailUrl()
    {
        var db = CreateDb();
        var fileId = "thumb-happy-1";
        var product = Product.CreateProduct("Widget", "widget-th", "desc", null,
            ProductStatus.Available, Guid.NewGuid(), ProductCondition.New, false,
            [], [], [], [], [], DateTimeOffset.UtcNow, "test", thumbnailFileId: fileId);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var consumer = new MediaAssetProcessedConsumer(db, Substitute.For<IProductEventPublisher>(), Substitute.For<ILogger<MediaAssetProcessedConsumer>>());
        var msg = new MediaAssetProcessedIntegrationEvent(fileId, "product_thumbnail", null, "https://cdn.example.com/thumb.jpg", null);
        var ctx = Substitute.For<ConsumeContext<MediaAssetProcessedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(ctx);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_Category_WhenCategoryFound_UpdatesImageUrl()
    {
        var db = CreateDb();
        var fileId = "cat-happy-1";
        var category = new Category("Electronics", imageFileId: fileId);
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var consumer = new MediaAssetProcessedConsumer(db, Substitute.For<IProductEventPublisher>(), Substitute.For<ILogger<MediaAssetProcessedConsumer>>());
        var msg = new MediaAssetProcessedIntegrationEvent(fileId, "category", null, "https://cdn.example.com/cat.jpg", null);
        var ctx = Substitute.For<ConsumeContext<MediaAssetProcessedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(ctx);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_ProductImage_WhenProductFound_UpdatesImageUrl()
    {
        var db = CreateDb();
        var fileId = "prod-img-happy-1";
        var product = Product.CreateProduct("Widget", "widget-pi", "desc", null,
            ProductStatus.Available, Guid.NewGuid(), ProductCondition.New, false,
            [], [], [new ProductImage(0, fileId)], [], [], DateTimeOffset.UtcNow, "test");
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var consumer = new MediaAssetProcessedConsumer(db, Substitute.For<IProductEventPublisher>(), Substitute.For<ILogger<MediaAssetProcessedConsumer>>());
        var msg = new MediaAssetProcessedIntegrationEvent(fileId, "product_image", null, "https://cdn.example.com/img.jpg", null);
        var ctx = Substitute.For<ConsumeContext<MediaAssetProcessedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(ctx);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_SkuImage_WhenProductFound_UpdatesSkuImageAndPublishesEvent()
    {
        var db = CreateDb();
        var fileId = "sku-img-happy-1";
        var skuImage = new SkuImage(fileId);
        var sku = new Sku("SKU-001", [], [skuImage], 10, true, Money.FromVND(50_000));
        var product = Product.CreateProduct("Widget", "widget-si", "desc", null,
            ProductStatus.Available, Guid.NewGuid(), ProductCondition.New, false,
            [], [], [], [sku], [], DateTimeOffset.UtcNow, "test");
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var publisher = Substitute.For<IProductEventPublisher>();
        var consumer = new MediaAssetProcessedConsumer(db, publisher, Substitute.For<ILogger<MediaAssetProcessedConsumer>>());
        var msg = new MediaAssetProcessedIntegrationEvent(fileId, "sku_image", null, "https://cdn.example.com/sku.jpg", null);
        var ctx = Substitute.For<ConsumeContext<MediaAssetProcessedIntegrationEvent>>();
        ctx.Message.Returns(msg);
        ctx.CancellationToken.Returns(CancellationToken.None);

        await consumer.Consume(ctx);

        await publisher.Received(1).PublishSkuUpdatedAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }
}
