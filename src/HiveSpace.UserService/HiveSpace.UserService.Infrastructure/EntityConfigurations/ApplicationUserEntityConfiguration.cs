using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.UserService.Domain.Enums;
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

        builder.Property(u => u.AvatarFileId)
            .HasMaxLength(100);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(2048);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.DateOfBirth);

        builder.Property(u => u.Gender);

        builder.Property(u => u.Status)
            .IsRequired();

        builder.Property(u => u.StoreId);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt);

        builder.Property(u => u.LastLoginAt);

        // Configure settings properties
        builder.Property(u => u.Theme)
            .HasConversion(
                v => MapTheme(v),
                v => ParseTheme(v))
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(u => u.Culture)
            .HasConversion(
                v => MapCulture(v),
                v => ParseCulture(v))
            .HasMaxLength(5)
            .IsRequired();

        // Configure ISoftDeletable properties
        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.DeletedAt);

        // Configure the RoleName property
        builder.Property(u => u.RoleName)
            .HasMaxLength(50);

        // Create index on RoleName for better query performance
        builder.HasIndex(u => u.RoleName);

        // Configure the addresses collection
        builder.HasMany(u => u.Addresses)
            .WithOne()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        // Table name
        builder.ToTable("users");
    }

    private static string MapTheme(Theme theme)
    {
        return theme switch
        {
            Theme.Light => "light",
            Theme.Dark => "dark",
            _ => throw new InvalidOperationException($"Unsupported theme value '{theme}'.")
        };
    }

    private static Theme ParseTheme(string value)
    {
        return value switch
        {
            "light" => Theme.Light,
            "dark" => Theme.Dark,
            _ => throw new InvalidOperationException($"Unsupported persisted theme value '{value}'.")
        };
    }

    private static string MapCulture(Culture culture)
    {
        return culture.ToCode();
    }

    private static Culture ParseCulture(string value)
    {
        return CultureExtensions.FromCode(value);
    }
}
