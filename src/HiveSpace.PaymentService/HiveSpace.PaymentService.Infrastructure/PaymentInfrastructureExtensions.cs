using HiveSpace.Core.Exceptions.Models;
using HiveSpace.Core.Exceptions;
using HiveSpace.Infrastructure.Persistence;
using HiveSpace.Infrastructure.Persistence.Seeding;
using HiveSpace.PaymentService.Domain.Repositories;
using HiveSpace.PaymentService.Domain.Services;
using HiveSpace.PaymentService.Infrastructure.Data;
using HiveSpace.PaymentService.Infrastructure.Gateways;
using HiveSpace.PaymentService.Infrastructure.Repositories;
using HiveSpace.PaymentService.Infrastructure.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.PaymentService.Infrastructure;

public static class PaymentInfrastructureExtensions
{
    public static void AddPaymentDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PaymentServiceDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var error = new Error(CommonErrorCode.ConfigurationMissing, "PaymentServiceDb");
            throw new HiveSpace.Core.Exceptions.ApplicationException([error], 500, false);
        }

        services.AddPersistenceInfrastructure<PaymentDbContext>();
        services.AddAppInterceptors();
        services.AddPaymentServiceRepositories();
        services.AddPaymentGateways(configuration);

        services.AddDbContext<PaymentDbContext>((serviceProvider, options) =>
        {
            var interceptors = serviceProvider.GetServices<ISaveChangesInterceptor>();
            options.UseSqlServer(connectionString)
                .AddInterceptors(interceptors);
        });

        services.AddDbContextFactory<PaymentDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(connectionString);
        }, ServiceLifetime.Scoped);

        services.AddScoped<DbContext>(provider => provider.GetRequiredService<PaymentDbContext>());
    }

    public static void AddPaymentServiceRepositories(this IServiceCollection services)
    {
        services.AddScoped<IPaymentRepository, SqlPaymentRepository>();
        services.AddScoped<IWalletRepository, SqlWalletRepository>();
        services.AddScoped<ISeeder, WalletSeeder>();
    }

    public static void AddPaymentGateways(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<VNPayConfiguration>(configuration.GetSection(VNPayConfiguration.SectionName));
        services.AddScoped<IPaymentGateway, VNPayGateway>();
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
    }
}
