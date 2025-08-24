using HiveSpace.UserService.Domain.Aggregates.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HiveSpace.UserService.Infrastructure.EntityConfigurations;

public class AdminEntityConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.HasKey(a => a.Id);

        // Email owned type
        builder.OwnsOne(a => a.Email, emailBuilder =>
        {
            emailBuilder.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(255);
        });
        
        builder.Property(a => a.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(a => a.Status)
            .HasConversion<string>()
            .IsRequired();
        
        builder.Property(a => a.IsSystem)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UpdatedAt);
        
        builder.Property(a => a.LastLoginAt);

        // Table name
        builder.ToTable("admins");
    }
}
