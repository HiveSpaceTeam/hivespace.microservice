using HiveSpace.AppHost.Extensions;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

const string httpLaunchProfile = "http";

var sqlPassword = builder.AddParameter(
    "sqlpassword",
    () => RequireNonEmptyConfiguration(builder.Configuration, "AppHost:SqlServer:Password"),
    secret: true);

var rabbitMqUsername = builder.AddParameter(
    "rabbitmqusername",
    () => RequireNonEmptyConfiguration(builder.Configuration, "AppHost:RabbitMq:Username"));

var rabbitMqPassword = builder.AddParameter(
    "rabbitmqpassword",
    () => RequireNonEmptyConfiguration(builder.Configuration, "AppHost:RabbitMq:Password"),
    secret: true);

var redisPassword = builder.AddParameter(
    "redispassword",
    () => RequireNonEmptyConfiguration(builder.Configuration, "AppHost:Redis:Password"),
    secret: true);

var mediaFuncPath = Path.GetFullPath(Path.Combine(
    builder.AppHostDirectory,
    "..",
    "HiveSpace.MediaService",
    "HiveSpace.MediaService.Func"));
var catalogFuncPath = Path.GetFullPath(Path.Combine(
    builder.AppHostDirectory,
    "..",
    "HiveSpace.CatalogService",
    "HiveSpace.CatalogService.Func"));

var mssql = builder.AddSqlServer("sql-server", sqlPassword, port: 1433)
    .WithDataVolume()
    .WithPersistentRestart();

// Aspire uses the database resource name for ConnectionStrings__{name} injection,
// so these stay PascalCase to match the appsettings connection keys.
var identityDb = mssql.AddDatabase("IdentityDb");
var userDb = mssql.AddDatabase("UserDb");
var catalogDb = mssql.AddDatabase("CatalogDb");
var mediaDb = mssql.AddDatabase("MediaDb");
var orderDb = mssql.AddDatabase("OrderDb");
var paymentDb = mssql.AddDatabase("PaymentDb");
var notificationDb = mssql.AddDatabase("NotificationDb");

var rabbitMq = builder.AddRabbitMQ("rabbit-mq", rabbitMqUsername, rabbitMqPassword, port: 5672)
    .WithDataVolume()
    .WithManagementPlugin()
    .WithPersistentRestart();

if (builder.Configuration.GetValue("AppHost:Kafka:Enabled", false))
{
    builder.AddKafka("kafka", port: 9092)
        .WithEnvironment("KAFKA_ADVERTISED_LISTENERS", "PLAINTEXT://localhost:29092,PLAINTEXT_HOST://127.0.0.1:9092,PLAINTEXT_INTERNAL://kafka:9093")
        .WithDataVolume()
        .WithPersistentRestart();
}

var redis = builder.AddRedis("redis", port: 6379, redisPassword)
    .WithDataVolume()
    .WithPersistentRestart();

var storage = builder.AddAzureStorage("azure-storage")
    .RunAsEmulator(emulator =>
    {
        emulator
            .WithBlobPort(10000)
            .WithQueuePort(10001)
            .WithTablePort(10002)
            .WithPersistentRestart();
    });
var blobStorage = storage.AddBlobs("azure-blob-storage");
var queueStorage = storage.AddQueues("azure-queue-storage");

var identity = builder.AddProject<Projects.HiveSpace_IdentityService_Api>("identity-service")
    .WithHttpEndpoint(port: 5001, targetPort: 5001, name: httpLaunchProfile, isProxied: false)
    .WithHttpEndpoint(port: 50011, targetPort: 50011, name: "grpc", isProxied: false)
    .WithReference(identityDb)
    .WithReference(rabbitMq, "RabbitMq")
    .WaitFor(identityDb)
    .WaitFor(rabbitMq);

var identityEndpoint = identity.GetEndpoint(httpLaunchProfile);

var user = builder.AddProject<Projects.HiveSpace_UserService_Api>("user-service")
    .WithHttpEndpoint(port: 5007, targetPort: 5007, name: httpLaunchProfile, isProxied: false)
    .WithHttpEndpoint(port: 50017, targetPort: 50017, name: "grpc", isProxied: false)
    .WithReference(userDb)
    .WithReference(rabbitMq, "RabbitMq")
    .WithEnvironment("Authentication__Authority", identityEndpoint)
    .WithEnvironment("InternalGrpc__IdentityService__Address", identity.GetEndpoint("grpc"))
    .WithEnvironment("InternalGrpc__IdentityService__TokenEndpoint", $"{identityEndpoint}/connect/token")
    .WaitFor(userDb)
    .WaitFor(rabbitMq)
    .WaitFor(identity);

var catalog = builder.AddProject<Projects.HiveSpace_CatalogService_Api>("catalog-service", httpLaunchProfile)
    .WithReference(catalogDb)
    .WithReference(rabbitMq, "RabbitMq")
    .WithEnvironment("Authentication__Authority", identityEndpoint)
    .WithEnvironment("InternalGrpc__UserService__Address", user.GetEndpoint("grpc"))
    .WithEnvironment("HIVESPACE_FUNCTION_QUEUE_MODE", "RabbitMQ")
    .WaitFor(catalogDb)
    .WaitFor(rabbitMq)
    .WaitFor(identity);

var media = builder.AddProject<Projects.HiveSpace_MediaService_Api>("media-service", httpLaunchProfile)
    .WithReference(rabbitMq, "RabbitMq")
    .WithReference(mediaDb)
    .WithReference(blobStorage, "AzureBlobStorage")
    .WithReference(queueStorage, "AzureQueueStorage")
    .WithEnvironment("Authentication__Authority", identityEndpoint)
    .WithEnvironment("HIVESPACE_FUNCTION_QUEUE_MODE", "RabbitMQ")
    .WaitFor(mediaDb)
    .WaitFor(rabbitMq)
    .WaitFor(storage)
    .WaitFor(identity);

builder.AddExecutable("media-func", "func", mediaFuncPath, "start", "--port", "7072", "--no-build", "--verbose")
    .WithReference(rabbitMq, "RabbitMq")
    .WithReference(mediaDb)
    .WithReference(blobStorage, "AzureBlobStorage")
    .WithReference(queueStorage, "AzureQueueStorage")
    .WithEnvironment("FUNCTIONS_WORKER_RUNTIME", "dotnet-isolated")
    .WithEnvironment("AzureWebJobsStorage", "UseDevelopmentStorage=true")
    .WithEnvironment("HIVESPACE_FUNCTION_QUEUE_MODE", "RabbitMQ")
    .WithEnvironment("Messaging__EnableRabbitMq", "true")
    .WaitFor(storage)
    .WaitFor(rabbitMq)
    .WaitFor(mediaDb);

// builder.AddExecutable("catalog-func", "func", catalogFuncPath, "start", "--port", "7073", "--no-build", "--verbose", "--script-root", "bin\\Debug\\net8.0")
//     .WithReference(rabbitMq, "RabbitMq")
//     .WithReference(catalogDb)
//     .WithEnvironment("FUNCTIONS_WORKER_RUNTIME", "dotnet-isolated")
//     .WithEnvironment("AzureWebJobsStorage", "UseDevelopmentStorage=true")
//     .WithEnvironment("HIVESPACE_FUNCTION_QUEUE_MODE", "RabbitMQ")
//     .WithEnvironment("AzureWebJobs.CatalogImportRabbitMqFunction.Disabled", "false")
//     .WithEnvironment("AzureWebJobs.CatalogImportServiceBusFunction.Disabled", "true")
//     .WithEnvironment("Messaging__EnableRabbitMq", "true")
//     .WaitFor(rabbitMq)
//     .WaitFor(catalogDb);

var order = builder.AddProject<Projects.HiveSpace_OrderService_Api>("order-service", httpLaunchProfile)
    .WithReference(orderDb)
    .WithReference(rabbitMq, "RabbitMq")
    .WithEnvironment("Authentication__Authority", identityEndpoint)
    .WaitFor(orderDb)
    .WaitFor(rabbitMq)
    .WaitFor(identity);

var payment = builder.AddProject<Projects.HiveSpace_PaymentService_Api>("payment-service", httpLaunchProfile)
    .WithReference(paymentDb)
    .WithReference(rabbitMq, "RabbitMq")
    .WithEnvironment("Authentication__Authority", identityEndpoint)
    .WaitFor(paymentDb)
    .WaitFor(rabbitMq)
    .WaitFor(identity);

var notification = builder.AddProject<Projects.HiveSpace_NotificationService_Api>("notification-service", httpLaunchProfile)
    .WithReference(notificationDb)
    .WithReference(rabbitMq, "RabbitMq")
    .WithReference(redis, "Redis")
    .WithEnvironment("Authentication__Authority", identityEndpoint)
    .WaitFor(rabbitMq)
    .WaitFor(redis)
    .WaitFor(notificationDb)
    .WaitFor(identity);

builder.AddProject<Projects.HiveSpace_YarpApiGateway>("api-gateway", httpLaunchProfile)
    .WithReference(identity)
    .WaitFor(identity)
    .WithEnvironment("Authentication__Authority", identityEndpoint)
    .WithEnvironment("ReverseProxy__Clusters__identity-cluster__Destinations__destination1__Address", identityEndpoint)
    .WithEnvironment("ReverseProxy__Clusters__user-cluster__Destinations__destination1__Address", user.GetEndpoint(httpLaunchProfile))
    .WithEnvironment("ReverseProxy__Clusters__catalog-cluster__Destinations__destination1__Address", catalog.GetEndpoint(httpLaunchProfile))
    .WithEnvironment("ReverseProxy__Clusters__media-cluster__Destinations__destination1__Address", media.GetEndpoint(httpLaunchProfile))
    .WithEnvironment("ReverseProxy__Clusters__order-cluster__Destinations__destination1__Address", order.GetEndpoint(httpLaunchProfile))
    .WithEnvironment("ReverseProxy__Clusters__payment-cluster__Destinations__destination1__Address", payment.GetEndpoint(httpLaunchProfile))
    .WithEnvironment("ReverseProxy__Clusters__notification-cluster__Destinations__destination1__Address", notification.GetEndpoint(httpLaunchProfile));

builder.Build().Run();

static string RequireNonEmptyConfiguration(IConfiguration configuration, string key)
{
    var value = configuration[key];
    if (!string.IsNullOrWhiteSpace(value))
        return value!;

    throw new InvalidOperationException($"Missing or empty {key}");
}
