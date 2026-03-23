using HiveSpace.CatalogService.Api.Consumers;
using HiveSpace.CatalogService.Api.Consumers.Saga.Checkout;
using HiveSpace.CatalogService.Infrastructure;
using HiveSpace.CatalogService.Infrastructure.Data;
using HiveSpace.Core;
using HiveSpace.Infrastructure.Messaging.Extensions;
using HiveSpace.Infrastructure.Persistence;
using HiveSpace.Infrastructure.Messaging.Configurations;

namespace HiveSpace.CatalogService.Api.Extentions
{
    internal static class HostingExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            builder.Services.AddAppApiControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddAppApiVersioning();
             builder.Services.AddAppApplicationServices();
            builder.Services.AddCatalogDbContext(configuration);
            
            // Add Core services for UserContext and other core functionality
            builder.Services.AddCoreServices();
            
            builder.Services.AddAppAuthentication(configuration);

            // Add Persistence services for TransactionService
            builder.Services.AddPersistenceInfrastructure<CatalogDbContext>();


            var messagingOptions = configuration.GetSection(MessagingOptions.SectionName).Get<MessagingOptions>();

            if (messagingOptions?.EnableKafka == true)
            {
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
            }

            if (messagingOptions?.EnableRabbitMq == true)
            {
                builder.Services.AddMassTransitWithRabbitMq<CatalogDbContext>(configuration, cfg =>
                {
                    cfg.AddConsumer<StoreCreatedConsumer>();
                    cfg.AddConsumer<ReserveInventoryConsumer>();
                    cfg.AddConsumer<ConfirmInventoryConsumer>();
                    cfg.AddConsumer<ReleaseInventoryConsumer>();
                });
            }

            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            return app;
        }
    }
}

