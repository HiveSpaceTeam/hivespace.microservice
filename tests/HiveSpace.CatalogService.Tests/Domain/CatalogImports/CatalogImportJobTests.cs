using FluentAssertions;
using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using HiveSpace.Domain.Shared.Exceptions;
using Xunit;

namespace HiveSpace.CatalogService.Tests.Domain.CatalogImports;

public class CatalogImportJobTests
{
    [Fact]
    public void Create_WithOperationAndRequester_StartsPending()
    {
        var requester = Guid.NewGuid();

        var job = CatalogImportJob.Create(
            CatalogImportJobOperationType.ProvisionCategories,
            "tiki",
            requester,
            "sha256:categories");

        job.Id.Should().NotBeEmpty();
        job.Status.Should().Be(CatalogImportJobStatus.Pending);
        job.OperationType.Should().Be(CatalogImportJobOperationType.ProvisionCategories);
        job.RequestedByUserId.Should().Be(requester);
        job.SourceFingerprint.Should().Be("sha256:categories");
        job.LastActivityAt.Should().Be(job.RequestedAt);
    }

    [Fact]
    public void Start_FromPending_SetsRunningAndStartedAt()
    {
        var job = CreateJob();

        job.Start();

        job.Status.Should().Be(CatalogImportJobStatus.Running);
        job.StartedAt.Should().NotBeNull();
        job.LastActivityAt.Should().Be(job.StartedAt);
    }

    [Fact]
    public void Complete_WithCounts_SetsCompletedResultSummary()
    {
        var job = CreateJob();
        job.Start();
        job.UpdateProgress(total: 10, processed: 10, created: 3, matched: 7);

        job.Complete("""{"created":3,"matched":7}""");

        job.Status.Should().Be(CatalogImportJobStatus.Completed);
        job.CompletedAt.Should().NotBeNull();
        job.LastActivityAt.Should().Be(job.CompletedAt);
        job.CreatedCount.Should().Be(3);
        job.MatchedCount.Should().Be(7);
        job.ResultSummaryJson.Should().Contain("created");
    }

    [Fact]
    public void Fail_WithErrorSummary_SetsFailed()
    {
        var job = CreateJob();
        job.Start();

        job.Fail("Tiki category payload was invalid.");

        job.Status.Should().Be(CatalogImportJobStatus.Failed);
        job.CompletedAt.Should().NotBeNull();
        job.LastActivityAt.Should().Be(job.CompletedAt);
        job.ErrorSummary.Should().Be("Tiki category payload was invalid.");
    }

    [Fact]
    public void Complete_FromPending_ThrowsDomainException()
    {
        var job = CreateJob();

        var act = () => job.Complete();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithoutRequester_ThrowsDomainException()
    {
        var act = () => CatalogImportJob.Create(
            CatalogImportJobOperationType.SubmitBundle,
            "tiki",
            Guid.Empty,
            "sha256:test");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateProgress_WithNegativeCount_ThrowsDomainException()
    {
        var job = CreateJob();
        job.Start();

        var act = () => job.UpdateProgress(total: 1, processed: -1);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Fail_WithoutErrorSummary_ThrowsDomainException()
    {
        var job = CreateJob();
        job.Start();

        var act = () => job.Fail("");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Requeue_FromFailed_ResetsProgressAndFailureState()
    {
        var job = CreateJob();
        job.Start();
        job.UpdateProgress(
            total: 10,
            processed: 8,
            created: 2,
            matched: 1,
            skipped: 1,
            blocked: 1,
            warnings: 1,
            duplicates: 1,
            failed: 1,
            conflicts: 1);
        job.Fail("Retryable provisioning failure.");

        job.Requeue();

        job.Status.Should().Be(CatalogImportJobStatus.Pending);
        job.StartedAt.Should().BeNull();
        job.CompletedAt.Should().BeNull();
        job.TotalCount.Should().Be(0);
        job.ProcessedCount.Should().Be(0);
        job.CreatedCount.Should().Be(0);
        job.MatchedCount.Should().Be(0);
        job.SkippedCount.Should().Be(0);
        job.BlockedCount.Should().Be(0);
        job.WarningCount.Should().Be(0);
        job.DuplicateCount.Should().Be(0);
        job.FailedCount.Should().Be(0);
        job.ConflictCount.Should().Be(0);
        job.ResultSummaryJson.Should().BeNull();
        job.ErrorSummary.Should().BeNull();
    }

    [Fact]
    public void Requeue_FromPending_ThrowsDomainException()
    {
        var job = CreateJob();

        var act = () => job.Requeue();

        act.Should().Throw<DomainException>();
    }

    private static CatalogImportJob CreateJob()
        => CatalogImportJob.Create(
            CatalogImportJobOperationType.SubmitBundle,
            "tiki",
            Guid.NewGuid(),
            $"sha256:{Guid.NewGuid():N}");
}
