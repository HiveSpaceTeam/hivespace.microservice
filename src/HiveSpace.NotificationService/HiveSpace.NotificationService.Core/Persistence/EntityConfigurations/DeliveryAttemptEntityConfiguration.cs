using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HiveSpace.NotificationService.Core.DomainModels;

namespace HiveSpace.NotificationService.Core.Persistence.EntityConfigurations;

public class DeliveryAttemptEntityConfiguration : IEntityTypeConfiguration<DeliveryAttempt>
{
    public void Configure(EntityTypeBuilder<DeliveryAttempt> builder)
    {
        builder.ToTable("delivery_attempts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.NotificationId).HasColumnName("notification_id").IsRequired();
        builder.Property(x => x.AttemptNumber).HasColumnName("attempt_number").IsRequired();
        builder.Property(x => x.AttemptedAt).HasColumnName("attempted_at").IsRequired();
        builder.Property(x => x.Success).HasColumnName("success").IsRequired();
        builder.Property(x => x.ProviderResponse).HasColumnName("provider_response").HasMaxLength(500);
        builder.Property(x => x.ErrorMessage).HasColumnName("error_message").HasMaxLength(1000);

        builder.HasIndex(x => x.NotificationId);
    }
}
