using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Aggregates.User;
using HiveSpace.UserService.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
        // IMPORTANT: Ignore the IdentityUserRole entity BEFORE calling base to avoid warning
        builder.Ignore<IdentityUserRole<Guid>>();

        base.OnModelCreating(builder);

        // Only apply configurations from Infrastructure assembly (not Domain)
        builder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);

        // Add MassTransit outbox entities
        MassTransitExtensions.AddEntityOutBox(builder);

        // Configure table names following the Identity Service pattern
        builder.Entity<Address>().ToTable("addresses");
        builder.Entity<IdentityRole<Guid>>().ToTable("roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");
        builder.Entity<Store>().ToTable("stores");
    }
}
