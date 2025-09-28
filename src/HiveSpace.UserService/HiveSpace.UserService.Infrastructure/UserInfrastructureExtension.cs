using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Infrastructure.Data;
using HiveSpace.UserService.Infrastructure.Repositories;
using HiveSpace.UserService.Infrastructure.DataQueries;
using HiveSpace.UserService.Application.Interfaces;
using HiveSpace.UserService.Application.Interfaces.DataQueries;
using HiveSpace.Infrastructure.Persistence;
using HiveSpace.Infrastructure.Persistence.Outbox;
using HiveSpace.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;

namespace HiveSpace.UserService.Infrastructure;

public static class UserInfrastructureExtension
{

    public static void AddUserDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("UserServiceDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var error = new Error(CommonErrorCode.ConfigurationMissing, "UserServiceDb");
            throw new HiveSpace.Core.Exceptions.ApplicationException(new[] { error }, 500, false);
        }

        // Register interceptors manually
        services.AddScoped<ISaveChangesInterceptor, AuditableInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, SoftDeleteInterceptor>();

        // Add persistence infrastructure to register other services
        services.AddPersistenceInfrastructure<UserDbContext>();

        // Add specific outbox repository for UserDbContext (for background services)
        services.AddOutboxServices<UserDbContext>();

        // Register UserService repositories
        services.AddUserServiceRepositories();

        // Register UserService queries with connection string
        services.AddUserServiceQueries(connectionString);

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

    public static void AddUserServiceQueries(this IServiceCollection services, string connectionString)
    {
        // Register Dapper Query services with connection string
        services.AddScoped<IUserDataQuery>(provider => new UserDataQuery(connectionString));
        services.AddScoped<IAdminDataQuery>(provider => new AdminDataQuery(connectionString));
    }
}
