using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.Domain.Shared.Enumerations;
using HiveSpace.UserService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HiveSpace.UserService.Infrastructure.EntityConfigurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        // Configure basic properties
        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => u.UserName)
            .IsUnique();

        builder.Property(u => u.Email)
            .HasConversion(
                v => v.Value,
                v => Email.Create(v))
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
            .HasConversion(new ValueConverter<PhoneNumber?, string?>(
                v => v == null ? null : v.Value,
                v => PhoneNumber.CreateOrDefault(v)))
            .HasMaxLength(20);

        builder.Property(u => u.DateOfBirth)
            .HasConversion(new ValueConverter<DateOfBirth?, DateTimeOffset?>(
                v => v == null ? null : v.Value,
                v => DateOfBirth.CreateOrDefault(v)));

        builder.Property(u => u.Gender)
            .HasConversion<int?>();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt);

        builder.OwnsOne(u => u.Settings, settings =>
        {
            settings.Property(s => s.Theme)
                .HasColumnName("Theme")
                .HasConversion(
                    v => MapTheme(v),
                    v => ParseTheme(v))
                .HasMaxLength(10)
                .IsRequired();

            settings.Property(s => s.Culture)
                .HasColumnName("Culture")
                .HasConversion(
                    v => MapCulture(v),
                    v => ParseCulture(v))
                .HasMaxLength(5)
                .IsRequired();
        });

        // Configure ISoftDeletable properties
        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.DeletedAt);

        // Configure the addresses collection
        builder.HasMany(u => u.Addresses)
            .WithOne()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(u => u.Addresses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

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
