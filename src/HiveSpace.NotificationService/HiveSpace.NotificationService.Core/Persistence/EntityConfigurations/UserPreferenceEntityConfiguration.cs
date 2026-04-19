using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HiveSpace.NotificationService.Core.DomainModels;

namespace HiveSpace.NotificationService.Core.Persistence.EntityConfigurations;

public class UserPreferenceEntityConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("user_preferences");

        builder.HasKey(x => new { x.UserId, x.Channel, x.EventType });

        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Channel).HasColumnName("channel").HasMaxLength(50).IsRequired()
               .HasConversion<string>();
        builder.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Enabled).HasColumnName("enabled").IsRequired().HasDefaultValue(true);
        builder.Property(x => x.QuietHoursJson).HasColumnName("quiet_hours_json");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
    }
}
