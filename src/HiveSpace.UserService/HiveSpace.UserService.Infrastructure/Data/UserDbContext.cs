using HiveSpace.UserService.Infrastructure.Identity;
using HiveSpace.UserService.Domain.Aggregates.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HiveSpace.UserService.Domain.Aggregates.Admin;
using HiveSpace.UserService.Domain.Aggregates.Store;

namespace HiveSpace.UserService.Infrastructure.Data;

public class UserDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Address> Addresses { get; set; }
    // Note: Admin and Store entities temporarily removed due to domain User references
    public DbSet<Admin> Admins { get; set; }
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
        builder.Entity<IdentityRole>().ToTable("roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("user_roles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("user_logins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("role_claims");
        builder.Entity<IdentityUserToken<string>>().ToTable("user_tokens");
        // Admin and Store tables temporarily removed
        builder.Entity<Admin>().ToTable("admins");
        builder.Entity<Store>().ToTable("stores");
    }
}
