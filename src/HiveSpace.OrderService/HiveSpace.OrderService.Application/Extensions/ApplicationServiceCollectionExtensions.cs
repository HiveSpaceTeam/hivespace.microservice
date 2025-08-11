using FluentValidation;
using HiveSpace.OrderService.Application.Mappers;
using HiveSpace.OrderService.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HiveSpace.OrderService.Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // AutoMapper
        services.AddAutoMapper(typeof(OrderMappingProfile).Assembly);

        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Application Services
        services.AddScoped<IOrderService, Services.OrderService>();

        return services;
    }
}