using HiveSpace.IdentityService.Core.DomainModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.IdentityService.Core.Persistence.EntityConfigurations;

public class ApplicationUserEntityConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("identity_users");

        builder.Property(u => u.FullName)
            .HasMaxLength(100);

        builder.Property(u => u.RoleName)
            .HasMaxLength(50);

        builder.HasIndex(u => u.RoleName);

        builder.Property(u => u.StoreId);

        builder.Property(u => u.Status)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.ActivatedAt);
        builder.Property(u => u.UpdatedAt);
        builder.Property(u => u.LastLoginAt);
    }
}
