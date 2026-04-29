using FluentValidation;
using HiveSpace.Application.Shared.Behaviors;
using HiveSpace.MediaService.Core.Features.Media.Commands.GeneratePresignedUrl;
using Microsoft.Extensions.DependencyInjection;

namespace HiveSpace.MediaService.Core;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<GeneratePresignedUrlCommand>();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<GeneratePresignedUrlCommand>();
            cfg.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });
        return services;
    }
}
