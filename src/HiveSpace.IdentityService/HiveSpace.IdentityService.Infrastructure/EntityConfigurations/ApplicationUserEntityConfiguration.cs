using HiveSpace.IdentityService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.IdentityService.Infrastructure.EntityConfigurations;

public class ApplicationUserEntityConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // Keep unique index on PhoneNumber
        builder.HasIndex(u => u.Email)
            .IsUnique();

        // Table name
        builder.ToTable("users");
    }
}