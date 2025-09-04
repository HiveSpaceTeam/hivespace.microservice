using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Infrastructure.Data;
using HiveSpace.UserService.Infrastructure.Repositories;
using HiveSpace.Infrastructure.Persistence;
using HiveSpace.Infrastructure.Persistence.Outbox;
using HiveSpace.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.UserService.Infrastructure;

public static class UserInfrastructureExtension
{

    public static void AddUserDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("UserServiceDb")
            ?? throw new InvalidOperationException($"Connection string 'UserServiceDb' not found.");

        // Register interceptors manually
        services.AddScoped<ISaveChangesInterceptor, AuditableInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, SoftDeleteInterceptor>();

        // Add persistence infrastructure to register other services
        services.AddPersistenceInfrastructure<UserDbContext>();

        // Add specific outbox repository for UserDbContext (for background services)
        services.AddOutboxServices<UserDbContext>();

        // Register UserService repositories
        services.AddUserServiceRepositories();

        services.AddDbContext<UserDbContext>((serviceProvider, options) =>
        {
            var interceptors = serviceProvider.GetServices<ISaveChangesInterceptor>();
            options.UseSqlServer(connectionString)      
                .AddInterceptors(interceptors);     
        });

        // Register the generic DbContext to resolve to UserDbContext
        // This is needed for services that depend on the generic DbContext type
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<UserDbContext>());
    }

    public static void AddUserServiceRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
    }
}
