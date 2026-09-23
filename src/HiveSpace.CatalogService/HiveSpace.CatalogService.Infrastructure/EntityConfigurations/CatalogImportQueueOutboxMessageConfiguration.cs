using HiveSpace.CatalogService.Domain.CatalogImports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.CatalogService.Infrastructure.EntityConfigurations;

public class CatalogImportQueueOutboxMessageConfiguration : IEntityTypeConfiguration<CatalogImportQueueOutboxMessage>
{
    public void Configure(EntityTypeBuilder<CatalogImportQueueOutboxMessage> builder)
    {
        builder.ToTable("catalog_import_queue_outbox_messages", table =>
            table.HasCheckConstraint("CK_catalog_import_queue_outbox_messages_attempt_positive", "[attempt] >= 1"));
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.JobId).IsRequired().HasColumnName("job_id");
        builder.Property(x => x.OperationType).IsRequired().HasColumnName("operation_type");
        builder.Property(x => x.Attempt).IsRequired().HasColumnName("attempt");
        builder.Property(x => x.CorrelationId).IsRequired().HasMaxLength(128).HasColumnName("correlation_id");
        builder.Property(x => x.QueuedAt).IsRequired().HasColumnName("queued_at");
        builder.Property(x => x.RequestedByUserId).IsRequired().HasColumnName("requested_by_user_id");
        builder.Property(x => x.SourceBundleId).HasColumnName("source_bundle_id");
        builder.Property(x => x.PayloadJson).IsRequired().HasColumnType("nvarchar(max)").HasColumnName("payload_json");
        builder.Property(x => x.LastAttemptAt).HasColumnName("last_attempt_at");
        builder.Property(x => x.DispatchedAt).HasColumnName("dispatched_at");
        builder.Property(x => x.FailureCount).HasColumnName("failure_count");
        builder.Property(x => x.LastError).HasMaxLength(2000).HasColumnName("last_error");

        builder.HasIndex(x => new { x.DispatchedAt, x.QueuedAt })
            .HasDatabaseName("IX_catalog_import_queue_outbox_messages_pending");
        builder.HasIndex(x => new { x.JobId, x.Attempt })
            .HasDatabaseName("IX_catalog_import_queue_outbox_messages_job_attempt");
    }
}
