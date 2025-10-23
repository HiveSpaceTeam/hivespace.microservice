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

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.LogoUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(s => s.Address)
            .HasMaxLength(500)
            .IsRequired();  

        builder.Property(s => s.OwnerId)
            .IsRequired();

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
