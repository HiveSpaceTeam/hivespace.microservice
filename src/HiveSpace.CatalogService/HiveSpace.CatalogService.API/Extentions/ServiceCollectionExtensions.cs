using FluentValidation;
using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Application.Services;
using HiveSpace.Core.Contexts;
using HiveSpace.Core.Filters;
using MediatR;
using HiveSpace.CatalogService.Application.Commands;
using HiveSpace.CatalogService.Application.Commands.Validators;
using HiveSpace.Infrastructure.Messaging.Behaviors;

namespace HiveSpace.CatalogService.API.Extentions
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
        public static void AddAppApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateProductCommand>());
            services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        }
    }
}
