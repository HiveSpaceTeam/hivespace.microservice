using HiveSpace.CatalogService.Domain.CatalogImports;
using HiveSpace.CatalogService.Domain.CatalogImports.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class CatalogImportJobConfiguration : IEntityTypeConfiguration<CatalogImportJob>
{
    public void Configure(EntityTypeBuilder<CatalogImportJob> builder)
    {
        builder.ToTable("catalog_import_jobs", table =>
            table.HasCheckConstraint("CK_catalog_import_jobs_attempt_positive", "[attempt] >= 1"));
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OperationType).IsRequired().HasColumnName("operation_type");
        builder.Property(x => x.Status).IsRequired().HasColumnName("status");
        builder.Property(x => x.Attempt).IsRequired().HasColumnName("attempt");
        builder.Property(x => x.SourceSystem).IsRequired().HasMaxLength(64).HasColumnName("source_system");
        builder.Property(x => x.SourceFingerprint).HasMaxLength(128).HasColumnName("source_fingerprint");
        builder.Property(x => x.SourceFileName).HasMaxLength(512).HasColumnName("source_file_name");
        builder.Property(x => x.BundleId).HasColumnName("bundle_id");
        builder.Property(x => x.RequestedByUserId).IsRequired().HasColumnName("requested_by_user_id");
        builder.Property(x => x.RequestedAt).IsRequired().HasColumnName("requested_at");
        builder.Property(x => x.LastActivityAt).IsRequired().HasColumnName("last_activity_at");
        builder.Property(x => x.StartedAt).HasColumnName("started_at");
        builder.Property(x => x.CompletedAt).HasColumnName("completed_at");
        builder.Property(x => x.TotalCount).HasColumnName("total_count");
        builder.Property(x => x.ProcessedCount).HasColumnName("processed_count");
        builder.Property(x => x.CreatedCount).HasColumnName("created_count");
        builder.Property(x => x.MatchedCount).HasColumnName("matched_count");
        builder.Property(x => x.SkippedCount).HasColumnName("skipped_count");
        builder.Property(x => x.BlockedCount).HasColumnName("blocked_count");
        builder.Property(x => x.WarningCount).HasColumnName("warning_count");
        builder.Property(x => x.DuplicateCount).HasColumnName("duplicate_count");
        builder.Property(x => x.FailedCount).HasColumnName("failed_count");
        builder.Property(x => x.ConflictCount).HasColumnName("conflict_count");
        builder.Property(x => x.ResultSummaryJson).HasColumnType("nvarchar(max)").HasColumnName("result_summary_json");
        builder.Property(x => x.ErrorSummary).HasMaxLength(2000).HasColumnName("error_summary");
        builder.Property(x => x.CorrelationId).HasMaxLength(128).HasColumnName("correlation_id");
        builder.Property(x => x.RequestPayloadJson).HasColumnType("nvarchar(max)").HasColumnName("request_payload_json");

        builder.HasIndex(x => new { x.OperationType, x.SourceSystem, x.SourceFingerprint })
            .HasFilter("[source_fingerprint] IS NOT NULL");

        builder.HasIndex(x => x.Status)
            .HasFilter($"[status] = {(int)CatalogImportJobStatus.Pending}");
    }
}
