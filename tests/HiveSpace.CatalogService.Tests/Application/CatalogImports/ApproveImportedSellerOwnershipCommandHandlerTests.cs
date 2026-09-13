using FluentAssertions;
using HiveSpace.CatalogService.Application.CatalogImports.Commands.ApproveImportedSellerOwnership;
using HiveSpace.CatalogService.Domain.Aggregates.External;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.Repositories.External;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Application.CatalogImports;

public class ApproveImportedSellerOwnershipCommandHandlerTests
{
    private const string SellerLogoUrl = "https://cdn.example.com/sellers/tiki-trading.png";

    [Fact]
    public async Task Handle_WithConflictedSellerAndEligibleStore_CreatesActiveOwnershipLink()
    {
        var targetUserId = Guid.NewGuid();
        var targetStoreId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var bundle = CreateConflictedBundle(targetUserId, targetStoreId);

        var result = await CreateHandler(bundle, new StoreRefRepositoryFake([
            new StoreRef(targetStoreId, targetUserId, "Existing Seed Store", null, null, "Seed address", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
        ])).Handle(
            new ApproveImportedSellerOwnershipCommand(bundle.Id, bundle.Sellers.Single().Id, targetUserId, targetStoreId, approverId, "Reviewed source seller"),
            CancellationToken.None);

        result.Status.Should().Be("Matched");
        result.UserId.Should().Be(targetUserId);
        result.StoreId.Should().Be(targetStoreId);
        result.ConflictReason.Should().BeNull();

        var seller = bundle.Sellers.Single();
        seller.HiveSpaceUserId.Should().Be(targetUserId);
        seller.HiveSpaceStoreId.Should().Be(targetStoreId);
        seller.ConflictReason.Should().BeNull();
        bundle.SellerOwnershipLinks.Should().ContainSingle(link =>
            link.ImportedSellerId == seller.Id
            && link.HiveSpaceUserId == targetUserId
            && link.HiveSpaceStoreId == targetStoreId
            && link.ApprovedByUserId == approverId
            && link.ApprovalReason == "Reviewed source seller");
    }

    [Fact]
    public async Task Handle_WithIneligibleTargetStore_ReturnsConflictAndKeepsSellerBlocked()
    {
        var targetUserId = Guid.NewGuid();
        var targetStoreId = Guid.NewGuid();
        var bundle = CreateConflictedBundle(Guid.NewGuid(), Guid.NewGuid());

        var result = await CreateHandler(bundle, new StoreRefRepositoryFake([
            new StoreRef(targetStoreId, targetUserId, "Existing Seed Store", null, null, "Seed address", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
        ])).Handle(
            new ApproveImportedSellerOwnershipCommand(bundle.Id, bundle.Sellers.Single().Id, targetUserId, targetStoreId, Guid.NewGuid(), "Wrong owner"),
            CancellationToken.None);

        result.Status.Should().Be("Conflict");
        result.ConflictReason.Should().Be("IneligibleSellerOwnershipTarget");
        bundle.Sellers.Single().ConflictReason.Should().Be("SimilarStoreNameConflict");
        bundle.SellerOwnershipLinks.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithApprovedLink_DoesNotUpdateExistingStoreProfileData()
    {
        var targetUserId = Guid.NewGuid();
        var targetStoreId = Guid.NewGuid();
        var bundle = CreateConflictedBundle(targetUserId, targetStoreId);
        var store = new StoreRef(targetStoreId, targetUserId, "Existing Seed Store", "Keep this", null, "Seed address", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        await CreateHandler(bundle, new StoreRefRepositoryFake([store])).Handle(
            new ApproveImportedSellerOwnershipCommand(bundle.Id, bundle.Sellers.Single().Id, targetUserId, targetStoreId, Guid.NewGuid(), "Approved"),
            CancellationToken.None);

        store.StoreName.Should().Be("Existing Seed Store");
        store.Description.Should().Be("Keep this");
        store.OwnerId.Should().Be(targetUserId);
    }

    [Fact]
    public async Task Handle_WithSellerNotInConflict_ReturnsConflictStatus()
    {
        var targetUserId = Guid.NewGuid();
        var targetStoreId = Guid.NewGuid();
        var bundle = CatalogImportBundle.Create(
            "2026-07-24",
            "tiki",
            "category",
            "1846",
            $"sha256:{Guid.NewGuid():N}",
            DateTimeOffset.UtcNow,
            Guid.NewGuid());
        bundle.AddSeller("seller-1", "Existing Seed Store", null, null, null, SellerLogoUrl)
            .MarkProvisioned(targetUserId, targetStoreId, created: false);

        var result = await CreateHandler(bundle, new StoreRefRepositoryFake([
            new StoreRef(targetStoreId, targetUserId, "Existing Seed Store", null, null, "Seed address", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
        ])).Handle(
            new ApproveImportedSellerOwnershipCommand(bundle.Id, bundle.Sellers.Single().Id, targetUserId, targetStoreId, Guid.NewGuid(), "Unexpected approval"),
            CancellationToken.None);

        result.Status.Should().Be("Conflict");
        result.ConflictReason.Should().Be("ImportedSellerDoesNotRequireApproval");
        bundle.SellerOwnershipLinks.Should().BeEmpty();
    }

    private static CatalogImportBundle CreateConflictedBundle(Guid suggestedUserId, Guid suggestedStoreId)
    {
        var bundle = CatalogImportBundle.Create(
            "2026-07-24",
            "tiki",
            "category",
            "1846",
            $"sha256:{Guid.NewGuid():N}",
            DateTimeOffset.UtcNow,
            Guid.NewGuid());
        bundle.AddSeller("seller-1", "Existing Seed Store", null, null, null, SellerLogoUrl)
            .MarkConflict("SimilarStoreNameConflict", suggestedStoreId, suggestedUserId);

        return bundle;
    }

    private static ApproveImportedSellerOwnershipCommandHandler CreateHandler(
        CatalogImportBundle bundle,
        IStoreRefRepository storeRefRepository)
        => new(new ValidateCatalogImportBundleCommandHandlerTests.CatalogImportBundleRepositoryFake(bundle), storeRefRepository);

    private sealed class StoreRefRepositoryFake(IReadOnlyCollection<StoreRef> stores) : IStoreRefRepository
    {
        public Task<StoreRef?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(stores.FirstOrDefault(x => x.Id == id));

        public Task AddAsync(StoreRef store, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
