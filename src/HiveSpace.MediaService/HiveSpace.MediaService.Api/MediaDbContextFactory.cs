using HiveSpace.MediaService.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace HiveSpace.MediaService.Api;

public class MediaDbContextFactory : IDesignTimeDbContextFactory<MediaDbContext>
{
    public MediaDbContext CreateDbContext(string[] args)
    {
        System.Console.WriteLine("----- USING MEDIA DB CONTEXT FACTORY -----");
        // Adjust path to find appsettings.json
        // If running from Solution Root
        var path = Path.Combine(Directory.GetCurrentDirectory(), "src/HiveSpace.MediaService/HiveSpace.MediaService.Api");
        
        if (!Directory.Exists(path)) 
        {
             // Maybe we are already in the Api directory?
             if (File.Exists("appsettings.json"))
                 path = Directory.GetCurrentDirectory();
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(path)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var builder = new DbContextOptionsBuilder<MediaDbContext>();
        var connectionString = configuration.GetConnectionString("MediaServiceDb");

        builder.UseSqlServer(connectionString, b => b.MigrationsAssembly("HiveSpace.MediaService.Core")); 

        return new MediaDbContext(builder.Options);
    }
}
