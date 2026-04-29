using FluentValidation;
using HiveSpace.Application.Shared.Behaviors;
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
            return services;
        }
    }
}
