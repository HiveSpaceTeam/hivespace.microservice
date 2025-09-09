using HiveSpace.UserService.Infrastructure.Identity;
using HiveSpace.UserService.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HiveSpace.UserService.Domain.Aggregates.Store;

namespace HiveSpace.UserService.Infrastructure.Data;

public class UserDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Store> Stores { get; set; }
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Only apply configurations from Infrastructure assembly (not Domain)
        builder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);

        // Configure table names following the Identity Service pattern
        builder.Entity<ApplicationUser>().ToTable("users");
        builder.Entity<Address>().ToTable("addresses");
        builder.Entity<IdentityRole<Guid>>().ToTable("roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
        builder.Entity<Store>().ToTable("stores");

        // Explicitly map ApplicationUser -> IdentityUserRole using UserId (avoid shadow FK ApplicationUserId)
        builder.Entity<ApplicationUser>()
            .HasMany(u => u.UserRoles)
            .WithOne() // IdentityUserRole<TKey> has no navigation back to user by default
            .HasForeignKey(ur => ur.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
