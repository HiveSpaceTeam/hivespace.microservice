using HiveSpace.Core;
using Carter;
using FluentValidation;
using Asp.Versioning;

// ReSharper disable CheckNamespace
namespace HiveSpace.MediaService.Api.Extensions
{
    internal static class HostingExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            
            builder.Services.AddAppApplicationServices();
            builder.Services.AddValidatorsFromAssembly(typeof(HostingExtensions).Assembly);
            builder.Services.AddAppPersistence(configuration);

            // Add Core services
            builder.Services.AddCoreServices();

            builder.Services.AddCarter();

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1);
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            
            // Authorization placeholder
            // app.UseAuthorization();
            
            var apiVersionSet = app.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(1))
                .ReportApiVersions()
                .Build();

            // Apply global prefix and versioning
            app.MapGroup("/api/v{version:apiVersion}")
                .WithApiVersionSet(apiVersionSet)
                .MapCarter();
            
            return app;
        }

    }
}
