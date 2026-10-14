using FluentValidation;
using HiveSpace.Application.Shared.Behaviors;
using HiveSpace.CatalogService.Application.CatalogImports.Jobs;
using HiveSpace.CatalogService.Application.CatalogImports.Queueing;
using HiveSpace.CatalogService.Application.Products.Commands.CreateProduct;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.CatalogService.Application
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<CreateProductCommand>();
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly);
                cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
            });
            services.AddScoped<ICatalogImportJobProcessor, CatalogImportJobProcessor>();
            return services;
        }

        public static IServiceCollection AddCatalogImportWorkerApplication(this IServiceCollection services)
        {
            services.AddScoped<CatalogImportFunctionDispatcher>();
            services.AddScoped<ICatalogImportJobProcessor, CatalogImportJobProcessor>();
            return services;
        }
    }
}
