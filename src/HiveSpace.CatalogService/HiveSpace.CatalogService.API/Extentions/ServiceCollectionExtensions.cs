﻿using HiveSpace.CatalogService.Application.Interfaces;
using HiveSpace.CatalogService.Application.Services;
using HiveSpace.Core.Contexts;
using HiveSpace.Core.Filters;

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
        }
    }
}
