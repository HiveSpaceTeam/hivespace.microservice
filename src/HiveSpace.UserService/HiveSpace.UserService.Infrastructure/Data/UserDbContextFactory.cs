using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

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

        var connectionString = configuration.GetConnectionString("UserServiceDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var error = new Error(CommonErrorCode.ConfigurationMissing, "UserServiceDb");
            throw new ConfigurationException([error]);
        }

        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        // Migrations are stored in the Infrastructure project migrations folder
        optionsBuilder.UseSqlServer(connectionString, options => options
            .EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null)
        );

        return new UserDbContext(optionsBuilder.Options);
    }
}
