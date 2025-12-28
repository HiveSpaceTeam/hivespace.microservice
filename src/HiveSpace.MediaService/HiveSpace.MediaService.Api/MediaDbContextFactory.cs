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
        // IMPORTANT: Migrations are in Core, so we must tell SqlServer where they live/should be.
        // Wait, if I run `migrations add --project Core`, the output is Core.
        // But the *Context* needs to know? 
        // "MigrationsAssembly" option in UseSqlServer is for *runtime* to find migrations table?
        // No, it's for *generating* migrations to know where to put them? No, that's `--project`.
        // It's for `database update` to know which assembly to check for Migration classes.
        // Since `MediaDbContext` is in `Core` and we want migrations in `Core`, usually `b.MigrationsAssembly("HiveSpace.MediaService.Core")` is good practice if Context is in one assembly but we might be running from another.
        // Given I am adding migration *into* Core, and the context is *in* Core, the default is Core.
        // So I might not strictly need it, but it's safer.

        return new MediaDbContext(builder.Options);
    }
}
