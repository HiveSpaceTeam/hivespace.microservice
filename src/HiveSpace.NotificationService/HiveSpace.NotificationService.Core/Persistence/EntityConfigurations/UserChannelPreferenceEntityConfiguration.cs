using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HiveSpace.NotificationService.Core.DomainModels;

namespace HiveSpace.NotificationService.Core.Persistence.EntityConfigurations;

public class UserChannelPreferenceEntityConfiguration : IEntityTypeConfiguration<UserChannelPreference>
{
    public void Configure(EntityTypeBuilder<UserChannelPreference> builder)
    {
        builder.ToTable("user_channel_preferences");

        builder.HasKey(x => new { x.UserId, x.Channel });

        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Channel).HasColumnName("channel").HasMaxLength(50).IsRequired()
               .HasConversion<string>();
        builder.Property(x => x.Enabled).HasColumnName("enabled").IsRequired().HasDefaultValue(true);
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
    }
}
