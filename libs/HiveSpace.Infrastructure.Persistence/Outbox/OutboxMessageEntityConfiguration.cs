using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.Infrastructure.Persistence.Outbox;

public class OutboxMessageEntityConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(x => x.Id);
        
        // Configure required fields with appropriate constraints
        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(4000); // Adjust based on your needs
            
        builder.Property(x => x.OccurredOnUtc)
            .IsRequired();
            
        builder.Property(x => x.ProcessedOnUtc);
        
        builder.Property(x => x.Error)
            .HasMaxLength(1000);
            
        builder.Property(x => x.Attempts)
            .IsRequired()
            .HasDefaultValue(0);
            
        // Configure audit fields
        builder.Property(x => x.CreatedAt)
            .IsRequired();
            
        builder.Property(x => x.UpdatedAt);
            
        // Configure table name following project conventions
        builder.ToTable("outbox_messages");
    }
} 