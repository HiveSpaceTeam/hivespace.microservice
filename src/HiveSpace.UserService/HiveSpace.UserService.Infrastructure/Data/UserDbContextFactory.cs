using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace HiveSpace.UserService.Infrastructure.Data;

public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        // Find the path to the application project where appsettings.json is located
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../HiveSpace.UserService.Api");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("UserServiceDb")
            ?? throw new InvalidOperationException("Connection string 'UserServiceDb' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new UserDbContext(optionsBuilder.Options);
    }
}
