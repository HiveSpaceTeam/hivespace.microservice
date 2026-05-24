using HiveSpace.UserService.Domain.Repositories;
using HiveSpace.UserService.Infrastructure.Data;
using HiveSpace.UserService.Infrastructure.Repositories;
using HiveSpace.UserService.Application.Interfaces;
using HiveSpace.Infrastructure.Persistence;
using HiveSpace.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HiveSpace.Core.Exceptions;
using HiveSpace.Core.Exceptions.Models;
using HiveSpace.UserService.Application.Interfaces.Messaging;
using HiveSpace.UserService.Infrastructure.Messaging.Publishers;

namespace HiveSpace.UserService.Infrastructure;

public static class UserInfrastructureExtension
{

    public static void AddUserDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("UserServiceDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var error = new Error(CommonErrorCode.ConfigurationMissing, "UserServiceDb");
            throw new HiveSpace.Core.Exceptions.ApplicationException([error], 500, false);
        }

        // Add persistence infrastructure to register other services
        services.AddPersistenceInfrastructure<UserDbContext>();

        services.AddAppInterceptors();

        // Register UserService repositories
        services.AddUserServiceRepositories();

        // Register Infrastructure services
        services.AddInfrastructureServices();

        // Register Event Publisher services
        services.AddEventPublisherServices();

        services.AddDbContext<UserDbContext>((serviceProvider, options) =>
        {
            var interceptors = serviceProvider.GetServices<ISaveChangesInterceptor>();
            options.UseSqlServer(
                connectionString, sqlOptions => sqlOptions
                .EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null))
                .AddInterceptors(interceptors);
        });

        // Register the generic DbContext to resolve to UserDbContext
        // This is needed for services that depend on the generic DbContext type
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<UserDbContext>());
    }

    public static void AddUserServiceRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, SqlUserRepository>();
        services.AddScoped<IStoreRepository, SqlStoreRepository>();
    }

    public static void AddInfrastructureServices(this IServiceCollection services)
    {
    }

    public static void AddEventPublisherServices(this IServiceCollection services)
    {
        services.AddScoped<IStoreEventPublisher, StoreEventPublisher>();
    }

}
