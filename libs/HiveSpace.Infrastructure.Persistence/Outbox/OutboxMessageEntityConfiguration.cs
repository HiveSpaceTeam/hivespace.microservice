using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.Infrastructure.Persistence.Outbox;

public class OutboxMessageEntityConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(x => x.EventId);

        // EventTypeName: Required, max length 500
        builder.Property(x => x.EventTypeName)
            .IsRequired()
            .HasMaxLength(500);

        // Content: Required, max length 4000
        builder.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(4000);

        // CreationTime: Required
        builder.Property(x => x.EventCreationTime)
            .IsRequired();

        // OperationId: Required
        builder.Property(x => x.OperationId)
            .IsRequired();

        // State: Required
        builder.Property(x => x.State)
            .IsRequired();

        // TimesSent: Required, default 0
        builder.Property(x => x.TimesSent)
            .IsRequired()
            .HasDefaultValue(0);

        // Ignore EventTypeShortName and IntegrationEvent (not mapped)
        builder.Ignore(x => x.EventTypeShortName);
        builder.Ignore(x => x.IntegrationEvent);

        builder.ToTable("outbox_messages");
    }
}