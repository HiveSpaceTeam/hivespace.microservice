using HiveSpace.IdentityService.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.IdentityService.Infrastructure.EntityConfigurations;

public class AddressEntityConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasKey(e => e.Id);

        // Configure required fields
        builder.Property(e => e.FullName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Street).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Ward).IsRequired().HasMaxLength(100);
        builder.Property(e => e.District).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Province).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Country).IsRequired().HasMaxLength(100);
        builder.Property(e => e.ZipCode).HasMaxLength(20);
        builder.Property(e => e.PhoneNumber).HasMaxLength(20);

        // Configure timestamps
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt);

        // Configure default value for IsDefault
        builder.Property(e => e.IsDefault).HasDefaultValue(false);

        // Table name
        builder.ToTable("user_addresses");
    }
} 