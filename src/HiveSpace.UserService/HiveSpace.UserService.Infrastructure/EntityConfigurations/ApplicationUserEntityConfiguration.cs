using HiveSpace.UserService.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.UserService.Infrastructure.EntityConfigurations;

public class ApplicationUserEntityConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasKey(u => u.Id);

        // Configure basic properties
        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(u => u.UserName)
            .IsUnique();
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.HasIndex(u => u.Email)
            .IsUnique();
        
        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);
        
        builder.Property(u => u.DateOfBirth);
        
        builder.Property(u => u.Gender)
            .HasConversion<string>();
        
        builder.Property(u => u.StoreId);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt);
        
        builder.Property(u => u.LastLoginAt);

        // Configure the addresses collection
        builder.HasMany(u => u.Addresses)
            .WithOne()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        // Table name
        builder.ToTable("users");
    }
}
