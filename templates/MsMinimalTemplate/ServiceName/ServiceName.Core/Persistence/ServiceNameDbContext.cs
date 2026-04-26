using Microsoft.EntityFrameworkCore;

namespace ServiceName.Core.Persistence;

public class ServiceNameDbContext(DbContextOptions<ServiceNameDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceNameDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
