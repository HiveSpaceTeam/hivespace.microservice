using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HiveSpace.NotificationService.Core.DomainModels;

namespace HiveSpace.NotificationService.Core.Persistence.EntityConfigurations;

public class UserGroupPreferenceEntityConfiguration : IEntityTypeConfiguration<UserGroupPreference>
{
    public void Configure(EntityTypeBuilder<UserGroupPreference> builder)
    {
        builder.ToTable("user_group_preferences");

        builder.HasKey(x => new { x.UserId, x.Channel, x.EventGroup });

        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Channel).HasColumnName("channel").HasMaxLength(50).IsRequired()
               .HasConversion<string>();
        builder.Property(x => x.EventGroup).HasColumnName("event_group").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Enabled).HasColumnName("enabled").IsRequired().HasDefaultValue(true);
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
    }
}
