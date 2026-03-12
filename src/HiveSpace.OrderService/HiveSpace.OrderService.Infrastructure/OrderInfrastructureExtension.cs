using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Core.Exceptions;
using HiveSpace.Infrastructure.Persistence;
using HiveSpace.OrderService.Application.Cart;
using HiveSpace.OrderService.Infrastructure.Data;
using HiveSpace.OrderService.Infrastructure.DataQueries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HiveSpace.OrderService.Domain.Repositories;
using HiveSpace.OrderService.Infrastructure.Repositories;

namespace HiveSpace.OrderService.Infrastructure;

public static class OrderInfrastructureExtension
{
    public static void AddOrderDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OrderServiceDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var error = new Error(CommonErrorCode.ConfigurationMissing, "OrderServiceDb");
            throw new HiveSpace.Core.Exceptions.ApplicationException([error], 500, false);
        }

        // Add persistence infrastructure to register other services
        services.AddPersistenceInfrastructure<OrderDbContext>();

        // Add specific outbox repository for OrderDbContext (if needed)
        // services.AddOutboxServices<OrderDbContext>();

        services.AddAppInterceptors();

        // Register OrderService repositories
        services.AddOrderServiceRepositories();

        // Register Infrastructure services
        services.AddInfrastructureServices();

        // Register Event Publisher services
        // services.AddEventPublisherServices();

        // Register OrderService queries
        services.AddOrderServiceQueries(connectionString);

        services.AddDbContext<OrderDbContext>((serviceProvider, options) =>
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

        // Register the generic DbContext to resolve to OrderDbContext
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<OrderDbContext>());
    }

    public static void AddOrderServiceRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICouponRepository, SqlCouponRepository>();
        services.AddScoped<ICartRepository, SqlCartRepository>();
        services.AddScoped<ISkuRefRepository, SqlSkuRefRepository>();
    }

    public static void AddOrderServiceQueries(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<ICartDataQuery>(_ => new CartDataQuery(connectionString));
    }

    public static void AddInfrastructureServices(this IServiceCollection services)
    {
        // TODO: Register Service Implementations here
    }
}
