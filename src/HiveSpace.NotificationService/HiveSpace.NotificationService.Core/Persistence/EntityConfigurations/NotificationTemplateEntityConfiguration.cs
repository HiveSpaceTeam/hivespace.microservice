using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HiveSpace.NotificationService.Core.DomainModels;

namespace HiveSpace.NotificationService.Core.Persistence.EntityConfigurations;

public class NotificationTemplateEntityConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("notification_templates");

        builder.HasKey(x => new { x.EventType, x.Channel, x.Locale });

        builder.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Channel).HasColumnName("channel").HasMaxLength(50).IsRequired()
               .HasConversion<string>();
        builder.Property(x => x.Locale).HasColumnName("locale").HasMaxLength(10).IsRequired();
        builder.Property(x => x.Subject).HasColumnName("subject").HasMaxLength(300).IsRequired();
        builder.Property(x => x.BodyTemplate).HasColumnName("body_template").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
    }
}
