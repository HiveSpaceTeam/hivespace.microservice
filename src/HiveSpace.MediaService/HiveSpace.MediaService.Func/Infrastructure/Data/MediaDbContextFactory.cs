using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HiveSpace.MediaService.Func.Infrastructure.Data;

public class MediaDbContextFactory : IDesignTimeDbContextFactory<MediaDbContext>
{
    public MediaDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration["Database:MediaServiceDb"];

        var optionsBuilder = new DbContextOptionsBuilder<MediaDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new MediaDbContext(optionsBuilder.Options);
    }
}
