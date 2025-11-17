using Confluent.Kafka;
using HiveSpace.CatalogService.Application.Consumers;
using HiveSpace.CatalogService.Application.IntegrationEvents;
using HiveSpace.CatalogService.Infrastructure;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Core;
using HiveSpace.Infrastructure.Persistence;
using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.CatalogService.Infrastructure.Messaging.Consumers;
using HiveSpace.Infrastructure.Messaging.Configurations;
using MassTransit;
using Microsoft.Extensions.Options;

namespace HiveSpace.CatalogService.API.Extentions
{
    internal static class HostingExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            builder.Services.AddAppApiControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddAppApplicationServices();
            builder.Services.AddCatalogDbContext(configuration);
            
            // Add Core services for UserContext and other core functionality
            builder.Services.AddCoreServices();
            
            // Add Persistence services for TransactionService
            builder.Services.AddPersistenceInfrastructure<CatalogDbContext>();

            builder.Services.AddMassTransitWithKafka(configuration,
                rider =>
                {
                    rider.AddConsumer<ProductAnalyticsConsumer>();
                    rider.AddConsumer<ProductAuditConsumer>();
                },
                (kafka, ctx) =>
                {
                    var kafkaOptions = ctx.GetRequiredService<IOptions<KafkaOptions>>().Value;

                    kafka.TopicEndpoint<Ignore, ProductCreatedIntegrationEvent>("catalog-product-created", kafkaOptions.ConsumerGroup, e =>
                    {
                        e.ConfigureConsumer<ProductAnalyticsConsumer>(ctx);
                    });

                    kafka.TopicEndpoint<Ignore, ProductUpdatedIntegrationEvent>("catalog-product-updated", kafkaOptions.ConsumerGroup, e =>
                    {
                        e.ConfigureConsumer<ProductAuditConsumer>(ctx);
                    });
                });

            builder.Services.AddMassTransitWithRabbitMq(configuration, cfg =>
            {
                cfg.AddConsumer<UserCreatedConsumer>();
            });

            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            return app;
        }
    }
}

