using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HiveSpace.NotificationService.Core.DomainModels.External;

namespace HiveSpace.NotificationService.Core.Persistence.EntityConfigurations;

public class UserRefEntityConfiguration : IEntityTypeConfiguration<UserRef>
{
    public void Configure(EntityTypeBuilder<UserRef> builder)
    {
        builder.ToTable("user_refs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(256).IsRequired();
        builder.Property(x => x.PhoneNumber).HasColumnName("phone_number").HasMaxLength(20);
        builder.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.UserName).HasColumnName("user_name").HasMaxLength(100);
        builder.Property(x => x.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(2048);
        builder.Property(x => x.StoreId).HasColumnName("store_id");
        builder.Property(x => x.StoreName).HasColumnName("store_name").HasMaxLength(200);
        builder.Property(x => x.StoreLogoUrl).HasColumnName("store_logo_url").HasMaxLength(2048);
        builder.Property(x => x.Locale).HasColumnName("locale").HasMaxLength(10).IsRequired()
               .HasDefaultValue("vi");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(x => x.Email);
    }
}
