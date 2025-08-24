using HiveSpace.UserService.Domain.Aggregates.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.UserService.Infrastructure.EntityConfigurations;

public class StoreEntityConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.StoreName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.StoreDescription)
            .HasMaxLength(500);

        builder.Property(s => s.LogoUrl)
            .HasMaxLength(500);

        builder.Property(s => s.OwnerId)
            .IsRequired();

        // Phone number owned type configuration
        builder.OwnsOne(s => s.ContactPhone, phoneBuilder =>
        {
            phoneBuilder.Property(p => p.Value)
                .HasColumnName("ContactPhone")
                .HasMaxLength(20);
        });

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .IsRequired();

        // Table name
        builder.ToTable("stores");
    }
}
