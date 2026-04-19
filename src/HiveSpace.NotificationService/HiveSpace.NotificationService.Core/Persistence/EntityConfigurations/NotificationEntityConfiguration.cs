using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HiveSpace.NotificationService.Core.DomainModels;

namespace HiveSpace.NotificationService.Core.Persistence.EntityConfigurations;

public class NotificationEntityConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Channel).HasColumnName("channel").IsRequired()
               .HasConversion<string>();
        builder.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(256).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").IsRequired()
               .HasConversion<string>();
        builder.Property(x => x.Payload).HasColumnName("payload").IsRequired()
               .HasDefaultValue("{}");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.SentAt).HasColumnName("sent_at");
        builder.Property(x => x.ReadAt).HasColumnName("read_at");
        builder.Property(x => x.ErrorMessage).HasColumnName("error_message").HasMaxLength(1000);
        builder.Property(x => x.AttemptCount).HasColumnName("attempt_count").IsRequired().HasDefaultValue(0);

        builder.HasIndex(x => x.IdempotencyKey).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.CreatedAt });

        builder.HasMany(x => x.Attempts)
               .WithOne()
               .HasForeignKey(a => a.NotificationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
