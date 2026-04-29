using HiveSpace.CatalogService.Api.Consumers.Saga.Checkout;
using HiveSpace.CatalogService.Api.Consumers.Sync;
using HiveSpace.CatalogService.Infrastructure;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Core;
using HiveSpace.Core.Extensions;
using HiveSpace.Core.Middlewares;
using HiveSpace.Infrastructure.Messaging.Configurations;
using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.Infrastructure.Persistence;
using Scalar.AspNetCore;

namespace HiveSpace.CatalogService.Api.Extensions
{
    internal static class HostingExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddAppApiControllers();
            builder.Services.AddAppOpenApi();
            builder.Services.AddAppApiVersioning();
            builder.Services.AddAppApplicationServices();
            builder.Services.AddCatalogDbContext(builder.Configuration);
            builder.Services.AddCoreServices();
            builder.Services.AddAppAuthentication(builder.Configuration);
            builder.Services.AddPersistenceInfrastructure<CatalogDbContext>();

            var messagingOptions = builder.Configuration
                .GetSection(MessagingOptions.SectionName)
                .Get<MessagingOptions>();

            if (messagingOptions?.EnableRabbitMq == true)
            {
                builder.Services.AddMassTransitWithRabbitMq<CatalogDbContext>(builder.Configuration, cfg =>
                {
                    cfg.AddConsumer<StoreRefSyncConsumer>();
                    cfg.AddConsumer<ReserveInventoryConsumer>();
                    cfg.AddConsumer<ConfirmInventoryConsumer>();
                    cfg.AddConsumer<ReleaseInventoryConsumer>();
                });
            }
            // if (messagingOptions?.EnableKafka == true)
            // {
                //builder.Services.AddMassTransitWithKafka(configuration,
                //    rider =>
                //    {
                //        //rider.AddConsumer<ProductAnalyticsConsumer>();
                //        rider.AddConsumer<ProductAuditConsumer>();
                //    },
                //    (kafka, ctx) =>
                //    {
                //        var kafkaOptions = ctx.GetRequiredService<IOptions<KafkaOptions>>().Value;

                //        //kafka.TopicEndpoint<Ignore, ProductCreatedIntegrationEvent>("catalog-product-created", kafkaOptions.ConsumerGroup, e =>
                //        //{
                //        //    e.ConfigureConsumer<ProductAnalyticsConsumer>(ctx);
                //        //});

                //        kafka.TopicEndpoint<Ignore, ProductUpdatedIntegrationEvent>("catalog-product-updated", kafkaOptions.ConsumerGroup, e =>
                //        {
                //            e.ConfigureConsumer<ProductAuditConsumer>(ctx);
                //        });
                //    });
            // }

            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.MapScalarApiReference(options => options
                    .WithTitle("HiveSpace CatalogService API")
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient));
            }

            app.UseHttpsRedirection();
            app.UseMiddleware<RequestIdMiddleware>();

            app.UseHiveSpaceExceptionHandler();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            return app;
        }
    }
}
