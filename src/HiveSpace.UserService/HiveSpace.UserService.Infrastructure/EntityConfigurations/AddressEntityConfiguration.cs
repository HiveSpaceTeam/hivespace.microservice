using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.UserService.Infrastructure.EntityConfigurations;

public class AddressEntityConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("addresses");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Street)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Commune)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Province)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.ZipCode)
            .HasMaxLength(20);

        builder.Property(a => a.AddressType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(a => a.IsDefault)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UpdatedAt);
    }
}
