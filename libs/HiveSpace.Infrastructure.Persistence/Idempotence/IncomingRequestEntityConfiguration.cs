using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.Infrastructure.Persistence.Idempotence;

public class IncomingRequestEntityConfiguration : IEntityTypeConfiguration<IncomingRequest>
{
    public void Configure(EntityTypeBuilder<IncomingRequest> builder)
    {
        builder.Property(x => x.DateTimeCreated).IsRequired();
        builder.Property(x => x.CorrelationId).IsRequired();
        builder.Property(x => x.RequestId).IsRequired();
        builder.Property(x => x.ActionName).IsRequired().HasMaxLength(256);
        builder.HasKey(o => new { o.RequestId, o.CorrelationId });

        builder.HasIndex(o => o.RequestId).IsUnique();

        builder.ToTable("incoming_requests");
    }
} 