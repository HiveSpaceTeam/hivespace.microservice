using Asp.Versioning;
using HiveSpace.CatalogService.Application;
using HiveSpace.CatalogService.Application.Commands;
using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Application.Queries;
using HiveSpace.CatalogService.Application.Services;
using HiveSpace.Core.Filters;
using HiveSpace.Infrastructure.Authorization.Extensions;

namespace HiveSpace.CatalogService.Api.Extentions
{
    internal static class ServiceCollectionExtensions
    {
        public static void AddAppApiControllers(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                options.Filters.Add<CustomExceptionFilter>();
            });
        }

        public static void AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Configure Authentication (Validate the Token)
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = configuration["Authentication:Authority"];
                    options.Audience = configuration["Authentication:Audience"];
                    options.RequireHttpsMetadata = configuration.GetValue<bool>("Authentication:RequireHttpsMetadata", true);
                });

            // 2. Configure Authorization (Check Permissions)
            services.AddHiveSpaceAuthorization("catalog.fullaccess");
        }

        public static void AddAppApplicationServices(this IServiceCollection services)
        {
            // Add MediatR and register handlers
            services.AddApplication();
            services.AddScoped<ICategoryService, CategoryService>();
        }

        public static void AddAppApiVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            });
        }
    }
}
