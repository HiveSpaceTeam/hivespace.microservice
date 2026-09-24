using FluentAssertions;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.SubmitCatalogImportBundle;
using HiveSpace.CatalogService.Application.CatalogImports.Dtos;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.CatalogService.Infrastructure.Repositories;
using HiveSpace.CatalogService.Tests.Fixtures;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports;

public class SubmitCatalogImportBundleCommandHandlerTests : IClassFixture<CatalogServiceFixture>
{
    private readonly CatalogServiceFixture _fixture;

    public SubmitCatalogImportBundleCommandHandlerTests(CatalogServiceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_WithNewSourceFingerprint_PersistsSubmittedBundle()
    {
        var repository = new SqlCatalogImportBundleRepository(_fixture.DbContext);
        repository.AddExternalCategoryLink(ExternalCategoryLink.Create(
            "tiki",
            "1846",
            "Nha sach Tiki",
            null,
            "[\"Nha sach Tiki\"]",
            1,
            "sha256:categories",
            Guid.NewGuid()));
        await repository.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(repository);
        var request = CreateRequest("sha256:new");

        var result = await handler.Handle(new SubmitCatalogImportBundleCommand(request), CancellationToken.None);
        await new CatalogImportJobProcessor(repository, new SqlCategoryRepository(_fixture.DbContext))
            .ProcessJobAsync(result.JobId, CancellationToken.None);
        var job = await repository.GetJobByIdAsync(result.JobId, CancellationToken.None);

        result.JobId.Should().NotBeEmpty();
        result.OperationType.Should().Be("SubmitBundle");
        result.Status.Should().Be("Pending");
        result.SourceFingerprint.Should().Be("sha256:new");
        job!.Status.ToString().Should().Be("Completed");
        job.BundleId.Should().NotBeNull();
        _fixture.DbContext.CatalogImportBundles.Should().ContainSingle(x => x.SourceFingerprint == "sha256:new");
        var storedBundle = await repository.GetByIdAsync(job.BundleId.Value, CancellationToken.None);
        storedBundle!.Sellers.Should().ContainSingle(x =>
            x.LogoUrl == "https://cdn.example.com/sellers/tiki-trading.png");
    }

    [Fact]
    public async Task Handle_WithExistingSourceFingerprint_ReturnsExistingBundle()
    {
        var handler = CreateHandler();
        var request = CreateRequest("sha256:existing");

        var first = await handler.Handle(new SubmitCatalogImportBundleCommand(request), CancellationToken.None);
        var second = await handler.Handle(new SubmitCatalogImportBundleCommand(request), CancellationToken.None);

        second.JobId.Should().Be(first.JobId);
        _fixture.DbContext.CatalogImportJobs.Should().ContainSingle(x => x.SourceFingerprint == "sha256:existing");
    }

    [Theory]
    [InlineData(CatalogImportJobStatus.Pending)]
    [InlineData(CatalogImportJobStatus.Running)]
    public async Task Handle_WithActiveExistingSourceFingerprint_ReturnsExistingJobWithoutScheduling(CatalogImportJobStatus status)
    {
        var repository = new SqlCatalogImportBundleRepository(_fixture.DbContext);
        var scheduler = new RecordingCatalogImportJobScheduler();
        var handler = CreateHandler(repository, scheduler);
        var request = CreateRequest($"sha256:active-{status}");

        var first = await handler.Handle(new SubmitCatalogImportBundleCommand(request), CancellationToken.None);
        if (status == CatalogImportJobStatus.Running)
        {
            var started = await repository.TryStartJobAsync(first.JobId, CancellationToken.None);
            started.Should().BeTrue();
        }
        scheduler.ScheduledJobs.Clear();

        var second = await handler.Handle(new SubmitCatalogImportBundleCommand(request), CancellationToken.None);

        second.JobId.Should().Be(first.JobId);
        scheduler.ScheduledJobs.Should().BeEmpty();
        _fixture.DbContext.CatalogImportJobs.Should().ContainSingle(x => x.SourceFingerprint == request.Crawl.SourceFingerprint);
    }

    [Fact]
    public async Task Handle_WithCompletedExistingSourceFingerprint_ReturnsExistingJobWithoutScheduling()
    {
        var repository = new SqlCatalogImportBundleRepository(_fixture.DbContext);
        repository.AddExternalCategoryLink(ExternalCategoryLink.Create(
            "tiki",
            "1846",
            "Nha sach Tiki",
            null,
            "[\"Nha sach Tiki\"]",
            1,
            "sha256:categories-completed",
            Guid.NewGuid()));
        await repository.SaveChangesAsync(CancellationToken.None);
        var scheduler = new RecordingCatalogImportJobScheduler();
        var handler = CreateHandler(repository, scheduler);
        var request = CreateRequest("sha256:completed");

        var first = await handler.Handle(new SubmitCatalogImportBundleCommand(request), CancellationToken.None);
        await new CatalogImportJobProcessor(repository, new SqlCategoryRepository(_fixture.DbContext))
            .ProcessJobAsync(first.JobId, CancellationToken.None);
        scheduler.ScheduledJobs.Clear();

        var second = await handler.Handle(new SubmitCatalogImportBundleCommand(request), CancellationToken.None);

        second.JobId.Should().Be(first.JobId);
        scheduler.ScheduledJobs.Should().BeEmpty();
        _fixture.DbContext.CatalogImportJobs.Should().ContainSingle(x => x.SourceFingerprint == "sha256:completed");
    }

    [Fact]
    public async Task Handle_WithFailedExistingSourceFingerprint_RequeuesExistingJob()
    {
        var repository = new SqlCatalogImportBundleRepository(_fixture.DbContext);
        var scheduler = new RecordingCatalogImportJobScheduler();
        var handler = CreateHandler(repository, scheduler);
        var request = CreateRequest("sha256:failed");

        var first = await handler.Handle(new SubmitCatalogImportBundleCommand(request), CancellationToken.None);
        var started = await repository.TryStartJobAsync(first.JobId, CancellationToken.None);
        started.Should().BeTrue();
        await repository.MarkJobFailedAsync(first.JobId, "failed during processing", CancellationToken.None);
        scheduler.ScheduledJobs.Clear();

        var second = await handler.Handle(new SubmitCatalogImportBundleCommand(request), CancellationToken.None);
        var job = await repository.GetJobByIdAsync(first.JobId, CancellationToken.None);

        second.JobId.Should().Be(first.JobId);
        second.Status.Should().Be("Pending");
        job!.Attempt.Should().Be(2);
        job.Status.Should().Be(CatalogImportJobStatus.Pending);
        scheduler.ScheduledJobs.Should().ContainSingle(x => x.Id == first.JobId && x.Attempt == 2);
        _fixture.DbContext.CatalogImportJobs.Should().ContainSingle(x => x.SourceFingerprint == "sha256:failed");
    }

    [Fact]
    public async Task Handle_WithDuplicateExternalProductIds_PersistsDuplicateGroupSummary()
    {
        var repository = new SqlCatalogImportBundleRepository(_fixture.DbContext);
        repository.AddExternalCategoryLink(ExternalCategoryLink.Create(
            "tiki",
            "1846",
            "Nha sach Tiki",
            null,
            "[\"Nha sach Tiki\"]",
            1,
            "sha256:categories-duplicates",
            Guid.NewGuid()));
        await repository.SaveChangesAsync(CancellationToken.None);

        var handler = CreateHandler(repository);
        var request = CreateRequest("sha256:duplicates") with
        {
            Products =
            [
                new ImportedProductRequestDto(
                    "product-1",
                    "seller-1",
                    ["1846"],
                    "https://tiki.vn/product-1",
                    "Imported Book",
                    null,
                    "https://cdn.tiki.vn/book.jpg",
                    [new ImportedAttributeRequestDto("Brand", "Tiki", null, null)],
                    [new ImportedImageRequestDto("https://cdn.tiki.vn/book.jpg", "Thumbnail", null)],
                    [],
                    [new ImportedSkuRequestDto("sku-1", "TIKI-SKU-1", new Dictionary<string, string>(), new ImportedPriceRequestDto(125000, "VND", "125000"), 12, [])]),
                new ImportedProductRequestDto(
                    "product-1",
                    "seller-1",
                    ["1846"],
                    "https://tiki.vn/product-1-copy",
                    "Imported Book Copy",
                    null,
                    "https://cdn.tiki.vn/book-copy.jpg",
                    [new ImportedAttributeRequestDto("Brand", "Tiki", null, null)],
                    [new ImportedImageRequestDto("https://cdn.tiki.vn/book-copy.jpg", "Thumbnail", null)],
                    [],
                    [new ImportedSkuRequestDto("sku-2", "TIKI-SKU-2", new Dictionary<string, string>(), new ImportedPriceRequestDto(126000, "VND", "126000"), 8, [])])
            ]
        };

        var result = await handler.Handle(new SubmitCatalogImportBundleCommand(request), CancellationToken.None);
        await new CatalogImportJobProcessor(repository, new SqlCategoryRepository(_fixture.DbContext))
            .ProcessJobAsync(result.JobId, CancellationToken.None);

        var bundle = await repository.GetBySourceFingerprintAsync("sha256:duplicates", CancellationToken.None);

        bundle.Should().NotBeNull();
        bundle!.DuplicateCount.Should().Be(1);
        bundle.DuplicateGroups.Should().ContainSingle(group => group.DuplicateKey == "tiki:product-1");
    }

    [Fact]
    public void Validate_WithMissingImportedSellerLogoUrl_ReturnsRequiredError()
    {
        var request = CreateRequest("sha256:missing-seller-logo") with
        {
            Sellers =
            [
                new ImportedSellerRequestDto(
                    "seller-1",
                    "Tiki Trading",
                    "tiki-trading",
                    "https://tiki.vn/cua-hang/tiki-trading",
                    null,
                    "")
            ]
        };

        var result = new SubmitCatalogImportBundleValidator()
            .Validate(new SubmitCatalogImportBundleCommand(request));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "Payload.Sellers[0].LogoUrl");
    }

    private SubmitCatalogImportBundleCommandHandler CreateHandler(
        SqlCatalogImportBundleRepository? repository = null,
        ICatalogImportJobScheduler? scheduler = null)
        => new(
            repository ?? new SqlCatalogImportBundleRepository(_fixture.DbContext),
            new FakeUserContext { UserId = Guid.NewGuid() },
            scheduler ?? new NullCatalogImportJobScheduler());

    private static CatalogImportBundleRequestDto CreateRequest(string sourceFingerprint)
        => new(
            "2026-07-24",
            new CatalogImportSourceDto("tiki", "category", "1846", "https://tiki.vn/1846"),
            new CatalogImportCrawlDto(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow, sourceFingerprint, null),
            [
                new ImportedSellerRequestDto(
                    "seller-1",
                    "Tiki Trading",
                    "tiki-trading",
                    "https://tiki.vn/cua-hang/tiki-trading",
                    null,
                    "https://cdn.example.com/sellers/tiki-trading.png")
            ],
            [],
            [
                new ImportedProductRequestDto(
                    "product-1",
                    "seller-1",
                    ["1846"],
                    "https://tiki.vn/product-1",
                    "Imported Book",
                    null,
                    "https://cdn.tiki.vn/book.jpg",
                    [new ImportedAttributeRequestDto("Brand", "Tiki", null, null)],
                    [new ImportedImageRequestDto("https://cdn.tiki.vn/book.jpg", "Thumbnail", null)],
                    [],
                    [new ImportedSkuRequestDto("sku-1", "TIKI-SKU-1", new Dictionary<string, string>(), new ImportedPriceRequestDto(125000, "VND", "125000"), 12, [])])
            ],
            [new ImportValidationHintRequestDto("Product", "product-1", "description", "Warning", "MissingOptionalDescription", "Description was not available.")]);

    private sealed class RecordingCatalogImportJobScheduler : ICatalogImportJobScheduler
    {
        public List<CatalogImportJob> ScheduledJobs { get; } = [];

        public Task ScheduleAsync(CatalogImportJob job, CancellationToken cancellationToken = default)
        {
            ScheduledJobs.Add(job);
            return Task.CompletedTask;
        }
    }
}
