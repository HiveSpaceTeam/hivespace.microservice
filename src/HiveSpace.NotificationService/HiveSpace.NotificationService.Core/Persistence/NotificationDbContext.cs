using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.NotificationService.Core.DomainModels;
using HiveSpace.NotificationService.Core.DomainModels.External;
using Microsoft.EntityFrameworkCore;

namespace HiveSpace.NotificationService.Core.Persistence;

public class NotificationDbContext(DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<Notification>         Notifications         => Set<Notification>();
    public DbSet<DeliveryAttempt>      DeliveryAttempts      => Set<DeliveryAttempt>();
    public DbSet<UserRef>              UserRefs              => Set<UserRef>();
    public DbSet<UserChannelPreference> UserChannelPreferences => Set<UserChannelPreference>();
    public DbSet<UserGroupPreference>   UserGroupPreferences   => Set<UserGroupPreference>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
        modelBuilder.AddEntityOutBox();
    }
}
