using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.UserService.Domain.Aggregates.Store;
using HiveSpace.UserService.Domain.Aggregates.User;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.UserService.Infrastructure.Data;

public class UserDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
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

        // Add MassTransit outbox entities
        MassTransitExtensions.AddEntityOutBox(builder);

        // Configure table names following the Identity Service pattern
        builder.Entity<Address>().ToTable("addresses");
        builder.Entity<Store>().ToTable("stores");
    }
}
