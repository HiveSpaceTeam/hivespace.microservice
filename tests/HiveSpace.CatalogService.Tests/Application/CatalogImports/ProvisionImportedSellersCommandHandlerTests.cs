using FluentAssertions;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.ProvisionImportedSellers;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Application.CatalogImports.Ports;
using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.Repositories.External;
using HiveSpace.Testing.Shared.Doubles;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports;

public class ProvisionImportedSellersCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithUnmatchedSeller_CallsIdentityThenUserProvisioningPorts()
    {
        var bundle = CreateBundle("provision-new");
        var repository = new CatalogImportBundleRepositoryFake(bundle);
        var accountClient = new RecordingImportedSellerAccountClient(
            new ImportedSellerAccountProvisioningResult(Guid.NewGuid(), ImportedSellerProvisioningOutcome.Created, null));
        var storeId = Guid.NewGuid();
        var storeClient = new RecordingImportedSellerStoreClient(
            new ImportedSellerStoreProvisioningResult(storeId, ImportedSellerProvisioningOutcome.Created, null));

        var job = await ExecuteProvisionAsync(repository, accountClient, storeClient, bundle.Id);

        job.CreatedCount.Should().Be(1);
        accountClient.Requests.Should().ContainSingle();
        storeClient.Requests.Should().ContainSingle();
        storeClient.Requests[0].UserId.Should().Be(accountClient.Response.UserId!.Value);
        storeClient.Requests[0].LogoUrl.Should().Be("https://cdn.example.com/sellers/tiki-trading.png");

        var storedSeller = bundle.Sellers.Single(x => x.ExternalSellerId == "seller-1");

        storedSeller.HiveSpaceUserId.Should().Be(accountClient.Response.UserId);
        storedSeller.HiveSpaceStoreId.Should().Be(storeId);
    }

    [Fact]
    public async Task Handle_WithCreatedStore_DoesNotBackfillStoreRef()
    {
        var bundle = CreateBundle("provision-store-ref-created");
        var repository = new CatalogImportBundleRepositoryFake(bundle);
        var accountClient = new RecordingImportedSellerAccountClient(
            new ImportedSellerAccountProvisioningResult(Guid.NewGuid(), ImportedSellerProvisioningOutcome.Created, null));
        var storeId = Guid.NewGuid();
        var storeClient = new RecordingImportedSellerStoreClient(
            new ImportedSellerStoreProvisioningResult(storeId, ImportedSellerProvisioningOutcome.Created, null));
        var storeRefs = new StoreRefRepositoryFake();

        await ExecuteProvisionAsync(repository, accountClient, storeClient, bundle.Id, storeRefs);

        storeRefs.Stores.Should().BeEmpty();

        var storedSeller = bundle.Sellers.Single(x => x.ExternalSellerId == "seller-1");
        storedSeller.HiveSpaceUserId.Should().Be(accountClient.Response.UserId);
        storedSeller.HiveSpaceStoreId.Should().Be(storeId);
    }

    [Fact]
    public async Task Handle_WithMatchedStore_BackfillsMissingStoreRef()
    {
        var bundle = CreateBundle("provision-store-ref-matched");
        var repository = new CatalogImportBundleRepositoryFake(bundle);
        var accountClient = new RecordingImportedSellerAccountClient(
            new ImportedSellerAccountProvisioningResult(Guid.NewGuid(), ImportedSellerProvisioningOutcome.Matched, null));
        var storeId = Guid.NewGuid();
        var storeClient = new RecordingImportedSellerStoreClient(
            new ImportedSellerStoreProvisioningResult(storeId, ImportedSellerProvisioningOutcome.Matched, null));
        var storeRefs = new StoreRefRepositoryFake();

        await ExecuteProvisionAsync(repository, accountClient, storeClient, bundle.Id, storeRefs);

        storeRefs.Stores.Should().ContainSingle(store =>
            store.Id == storeId
            && store.OwnerId == accountClient.Response.UserId!.Value
            && store.StoreName == "Tiki Trading");
    }

    [Fact]
    public async Task Handle_WithExistingOwnershipLink_SkipsAlreadyProvisionedSeller()
    {
        var bundle = CreateBundle("provision-existing");
        var repository = new CatalogImportBundleRepositoryFake(bundle);
        var accountClient = new RecordingImportedSellerAccountClient(
            new ImportedSellerAccountProvisioningResult(Guid.NewGuid(), ImportedSellerProvisioningOutcome.Created, null));
        var storeClient = new RecordingImportedSellerStoreClient(
            new ImportedSellerStoreProvisioningResult(Guid.NewGuid(), ImportedSellerProvisioningOutcome.Created, null));

        await ExecuteProvisionAsync(repository, accountClient, storeClient, bundle.Id);
        accountClient.Requests.Clear();
        storeClient.Requests.Clear();

        var job = await ExecuteProvisionAsync(repository, accountClient, storeClient, bundle.Id);

        job.SkippedCount.Should().Be(1);
        accountClient.Requests.Should().BeEmpty();
        storeClient.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithIdentityConflict_MarksSellerConflictAndBlocksProducts()
    {
        var bundle = CreateBundle("provision-identity-conflict");
        var repository = new CatalogImportBundleRepositoryFake(bundle);
        var accountClient = new RecordingImportedSellerAccountClient(
            new ImportedSellerAccountProvisioningResult(null, ImportedSellerProvisioningOutcome.Conflict, "IdentityConflict"));
        var storeClient = new RecordingImportedSellerStoreClient(
            new ImportedSellerStoreProvisioningResult(Guid.NewGuid(), ImportedSellerProvisioningOutcome.Created, null));

        var job = await ExecuteProvisionAsync(repository, accountClient, storeClient, bundle.Id);

        job.ConflictCount.Should().Be(1);
        storeClient.Requests.Should().BeEmpty();

        bundle.Sellers.Single().ConflictReason.Should().Be("IdentityConflict");
        bundle.ValidationIssues.Should().Contain(x => x.EntityType == "Seller" && x.EntitySourceId == "seller-1");
    }

    [Fact]
    public async Task Handle_WithStoreConflict_MarksSellerConflictAndBlocksProducts()
    {
        var bundle = CreateBundle("provision-store-conflict");
        var repository = new CatalogImportBundleRepositoryFake(bundle);
        var accountClient = new RecordingImportedSellerAccountClient(
            new ImportedSellerAccountProvisioningResult(Guid.NewGuid(), ImportedSellerProvisioningOutcome.Matched, null));
        var storeClient = new RecordingImportedSellerStoreClient(
            new ImportedSellerStoreProvisioningResult(null, ImportedSellerProvisioningOutcome.Conflict, "StoreNameConflict"));

        var job = await ExecuteProvisionAsync(repository, accountClient, storeClient, bundle.Id);

        job.ConflictCount.Should().Be(1);

        bundle.Sellers.Single().ConflictReason.Should().Be("StoreNameConflict");
    }

    [Fact]
    public async Task Handle_WithPendingProvisionJob_ReturnsExistingJob()
    {
        var bundle = CreateBundle("provision-active");
        var repository = new CatalogImportBundleRepositoryFake(bundle);

        var first = await new ProvisionImportedSellersCommandHandler(
            repository,
            new FakeUserContext { UserId = Guid.NewGuid() },
            new NullCatalogImportJobScheduler())
            .Handle(new ProvisionImportedSellersCommand(bundle.Id), CancellationToken.None);

        var second = await new ProvisionImportedSellersCommandHandler(
            repository,
            new FakeUserContext { UserId = Guid.NewGuid() },
            new NullCatalogImportJobScheduler())
            .Handle(new ProvisionImportedSellersCommand(bundle.Id), CancellationToken.None);

        second.JobId.Should().Be(first.JobId);
        repository.JobCount.Should().Be(1);
    }

    private static CatalogImportBundle CreateBundle(string sourceFingerprint)
    {
        var bundle = CatalogImportBundle.Create(
            "2026-07-24",
            "tiki",
            "category",
            "1846",
            sourceFingerprint,
            DateTimeOffset.UtcNow.AddMinutes(-5),
            Guid.NewGuid(),
            "https://tiki.vn/1846");

        bundle.AddSeller(
            "seller-1",
            "Tiki Trading",
            "tiki-trading",
            "https://tiki.vn/cua-hang/tiki-trading",
            null,
            "https://cdn.example.com/sellers/tiki-trading.png");

        return bundle;
    }

    private static async Task<CatalogImportJob> ExecuteProvisionAsync(
        CatalogImportBundleRepositoryFake repository,
        IImportedSellerAccountClient accountClient,
        IImportedSellerStoreClient storeClient,
        Guid bundleId,
        IStoreRefRepository? storeRefRepository = null)
    {
        var submission = await new ProvisionImportedSellersCommandHandler(
            repository,
            new FakeUserContext { UserId = Guid.NewGuid() },
            new NullCatalogImportJobScheduler())
            .Handle(new ProvisionImportedSellersCommand(bundleId), CancellationToken.None);

        await new CatalogImportJobProcessor(
            repository,
            new ValidateCatalogImportBundleCommandHandlerTests.CategoryRepositoryFake(),
            new ValidateCatalogImportBundleCommandHandlerTests.AttributeRepositoryFake(),
            accountClient: accountClient,
            storeClient: storeClient,
            storeRefRepository: storeRefRepository)
            .ProcessJobAsync(submission.JobId, CancellationToken.None);

        return (await repository.GetJobByIdAsync(submission.JobId, CancellationToken.None))!;
    }

    private sealed class CatalogImportBundleRepositoryFake(CatalogImportBundle bundle) : ICatalogImportBundleRepository
    {
        private readonly List<CatalogImportJob> _jobs = [];
        private readonly List<ExternalCategoryLink> _categoryLinks = [];

        public void Add(CatalogImportBundle bundle)
        {
        }

        public void AddJob(CatalogImportJob job)
        {
            _jobs.Add(job);
        }

        public int JobCount => _jobs.Count;

        public void AddExternalCategoryLink(ExternalCategoryLink categoryLink)
            => _categoryLinks.Add(categoryLink);

        public void AddExternalCategoryAttributeLink(ExternalCategoryAttributeLink categoryAttributeLink)
        {
        }

        public Task<ExternalCategoryLink?> GetExternalCategoryLinkAsync(
            string sourceSystem,
            string externalCategoryId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_categoryLinks.FirstOrDefault(x =>
                string.Equals(x.SourceSystem, sourceSystem, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.ExternalCategoryId, externalCategoryId, StringComparison.OrdinalIgnoreCase)));

        public Task<IReadOnlyList<ExternalCategoryLink>> GetExternalCategoryLinksAsync(
            string sourceSystem,
            IEnumerable<string> externalCategoryIds,
            CancellationToken cancellationToken = default)
        {
            var ids = externalCategoryIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
            return Task.FromResult<IReadOnlyList<ExternalCategoryLink>>(
                _categoryLinks.Where(x => string.Equals(x.SourceSystem, sourceSystem, StringComparison.OrdinalIgnoreCase)
                    && ids.Contains(x.ExternalCategoryId)).ToList());
        }

        public Task<IReadOnlyList<ExternalCategoryAttributeLink>> GetExternalCategoryAttributeLinksAsync(
            string sourceSystem,
            IEnumerable<string> externalCategoryIds,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ExternalCategoryAttributeLink>>([]);

        public Task<int?> GetImportedProductIdBySourceIdentityAsync(
            string sourceSystem,
            string externalProductId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<int?>(null);

        public Task<CatalogImportBundle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<CatalogImportBundle?>(id == bundle.Id ? bundle : null);

        public Task<CatalogImportBundle?> GetBySourceFingerprintAsync(string sourceFingerprint, CancellationToken cancellationToken = default)
            => Task.FromResult<CatalogImportBundle?>(bundle.SourceFingerprint == sourceFingerprint ? bundle : null);

        public Task<CatalogImportJob?> GetJobByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_jobs.FirstOrDefault(x => x.Id == id));

        public Task<CatalogImportJob?> GetJobByOperationAndSourceFingerprintAsync(
            HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobOperationType operationType,
            string sourceSystem,
            string sourceFingerprint,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_jobs.FirstOrDefault(x =>
                x.OperationType == operationType
                && x.SourceSystem == sourceSystem
                && x.SourceFingerprint == sourceFingerprint));

        public Task<CatalogImportJob?> GetActiveJobByOperationAndSourceFingerprintAsync(
            HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobOperationType operationType,
            string sourceSystem,
            string sourceFingerprint,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_jobs.FirstOrDefault(x =>
                x.OperationType == operationType
                && x.SourceSystem == sourceSystem
                && x.SourceFingerprint == sourceFingerprint
                && x.Status is HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobStatus.Pending
                    or HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobStatus.Running));

        public Task<IReadOnlyList<CatalogImportJob>> GetPendingJobsAsync(int limit, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CatalogImportJob>>(_jobs
                .Where(x => x.Status == HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobStatus.Pending)
                .Take(limit)
                .ToList());

        public Task<IReadOnlyList<CatalogImportJob>> GetRunningJobsInactiveSinceAsync(
            DateTimeOffset inactiveSince,
            int limit,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CatalogImportJob>>(_jobs
                .Where(x =>
                    x.Status == HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobStatus.Running
                    && x.LastActivityAt <= inactiveSince)
                .Take(limit)
                .ToList());


        public Task<(IReadOnlyList<CatalogImportJob> Jobs, int TotalCount)> ListJobsAsync(
            int pageNumber,
            int pageSize,
            HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobStatus? status = null,
            HiveSpace.CatalogService.Domain.CatalogImports.Enums.CatalogImportJobOperationType? operationType = null,
            string? sourceSystem = null,
            Guid? bundleId = null,
            DateTimeOffset? requestedFrom = null,
            DateTimeOffset? requestedTo = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(((IReadOnlyList<CatalogImportJob>)_jobs, _jobs.Count));

        public Task<(IReadOnlyList<CatalogImportBundle> Bundles, int TotalCount)> ListBundlesAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
            => Task.FromResult(((IReadOnlyList<CatalogImportBundle>)[bundle], 1));

        public Task<IReadOnlyList<CatalogImportBundle>> ListRecentAsync(int limit, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CatalogImportBundle>>([bundle]);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(1);
    }

    private sealed class RecordingImportedSellerAccountClient(ImportedSellerAccountProvisioningResult response)
        : IImportedSellerAccountClient
    {
        public ImportedSellerAccountProvisioningResult Response { get; } = response;
        public List<ImportedSellerAccountProvisioningRequest> Requests { get; } = [];

        public Task<ImportedSellerAccountProvisioningResult> ProvisionAsync(
            ImportedSellerAccountProvisioningRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(Response);
        }
    }

    private sealed class RecordingImportedSellerStoreClient(ImportedSellerStoreProvisioningResult response)
        : IImportedSellerStoreClient
    {
        public List<ImportedSellerStoreProvisioningRequest> Requests { get; } = [];

        public Task<ImportedSellerStoreProvisioningResult> ProvisionAsync(
            ImportedSellerStoreProvisioningRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(response);
        }
    }

    private sealed class StoreRefRepositoryFake : IStoreRefRepository
    {
        public List<StoreRef> Stores { get; } = [];

        public Task<StoreRef?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Stores.FirstOrDefault(x => x.Id == id));

        public Task AddAsync(StoreRef store, CancellationToken cancellationToken = default)
        {
            Stores.Add(store);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
