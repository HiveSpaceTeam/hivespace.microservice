using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HiveSpace.NotificationService.Core.Persistence;

/// <summary>Used only by dotnet-ef at design time (migrations). Not used at runtime.</summary>
public class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=NotificationDb;User Id=sa;Password=Passw0rd123!;Encrypt=False;TrustServerCertificate=True");

        return new NotificationDbContext(optionsBuilder.Options);
    }
}
