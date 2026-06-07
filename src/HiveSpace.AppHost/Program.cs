using HiveSpace.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

const string httpLaunchProfile = "http";

var sqlPassword = builder.AddParameter(
    "sqlpassword",
    () => builder.Configuration["AppHost:SqlServer:Password"]
          ?? throw new InvalidOperationException("Missing AppHost:SqlServer:Password"),
    secret: true);

var rabbitMqUsername = builder.AddParameter(
    "rabbitmqusername",
    () => builder.Configuration["AppHost:RabbitMq:Username"]
          ?? throw new InvalidOperationException("Missing AppHost:RabbitMq:Username"));

var rabbitMqPassword = builder.AddParameter(
    "rabbitmqpassword",
    () => builder.Configuration["AppHost:RabbitMq:Password"]
          ?? throw new InvalidOperationException("Missing AppHost:RabbitMq:Password"),
    secret: true);

var redisPassword = builder.AddParameter(
    "redispassword",
    () => builder.Configuration["AppHost:Redis:Password"]
          ?? throw new InvalidOperationException("Missing AppHost:Redis:Password"),
    secret: true);

var mediaFuncPath = Path.GetFullPath(Path.Combine(
    builder.AppHostDirectory,
    "..",
    "HiveSpace.MediaService",
    "HiveSpace.MediaService.Func"));

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

var kafka = builder.AddKafka("kafka", port: 9092)
    .WithDataVolume()
    .WithPersistentRestart();

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

var identity = builder.AddProject<Projects.HiveSpace_IdentityService_Api>("identity-service", httpLaunchProfile)
    .WithReference(identityDb)
    .WithReference(rabbitMq, "RabbitMq")
    .WaitFor(identityDb);

var identityEndpoint = identity.GetEndpoint(httpLaunchProfile);

var user = builder.AddProject<Projects.HiveSpace_UserService_Api>("user-service", httpLaunchProfile)
    .WithReference(userDb)
    .WithReference(rabbitMq, "RabbitMq")
    .WithEnvironment("Authentication__Authority", identityEndpoint)
    .WaitFor(userDb);

var catalog = builder.AddProject<Projects.HiveSpace_CatalogService_Api>("catalog-service", httpLaunchProfile)
    .WithReference(catalogDb)
    .WithReference(rabbitMq, "RabbitMq")
    .WithEnvironment("Authentication__Authority", identityEndpoint)
    .WaitFor(catalogDb);

var media = builder.AddProject<Projects.HiveSpace_MediaService_Api>("media-service", httpLaunchProfile)
    .WithReference(rabbitMq, "RabbitMq")
    .WithReference(mediaDb)
    .WithReference(blobStorage, "AzureBlobStorage")
    .WithReference(queueStorage, "AzureQueueStorage")
    .WithEnvironment("Authentication__Authority", identityEndpoint)
    .WaitFor(mediaDb);

builder.AddExecutable("media-func", "func", mediaFuncPath, "start", "--port", "7072", "--no-build", "--verbose")
    .WithReference(rabbitMq, "RabbitMq")
    .WithReference(mediaDb)
    .WithReference(blobStorage, "AzureBlobStorage")
    .WithReference(queueStorage, "AzureQueueStorage")
    .WithEnvironment("FUNCTIONS_WORKER_RUNTIME", "dotnet-isolated")
    .WithEnvironment("AzureWebJobsStorage", "UseDevelopmentStorage=true")
    .WithEnvironment("Messaging__EnableRabbitMq", "true")
    .WaitFor(storage)
    .WaitFor(rabbitMq)
    .WaitFor(mediaDb);

var order = builder.AddProject<Projects.HiveSpace_OrderService_Api>("order-service", httpLaunchProfile)
    .WithReference(orderDb)
    .WithReference(rabbitMq, "RabbitMq")
    .WithEnvironment("Authentication__Authority", identityEndpoint)
    .WaitFor(orderDb);

var payment = builder.AddProject<Projects.HiveSpace_PaymentService_Api>("payment-service", httpLaunchProfile)
    .WithReference(paymentDb)
    .WithReference(rabbitMq, "RabbitMq")
    .WithEnvironment("Authentication__Authority", identityEndpoint)
    .WaitFor(paymentDb);

var notification = builder.AddProject<Projects.HiveSpace_NotificationService_Api>("notification-service", httpLaunchProfile)
    .WithReference(notificationDb)
    .WithReference(rabbitMq, "RabbitMq")
    .WithReference(redis, "Redis")
    .WithEnvironment("Authentication__Authority", identityEndpoint)
    .WaitFor(redis)
    .WaitFor(notificationDb);

builder.AddProject<Projects.HiveSpace_YarpApiGateway>("api-gateway", httpLaunchProfile)
    .WithReference(identity)
    .WithEnvironment("Authentication__Authority", identityEndpoint)
    .WithEnvironment("ReverseProxy__Clusters__identity-cluster__Destinations__destination1__Address", identityEndpoint)
    .WithEnvironment("ReverseProxy__Clusters__user-cluster__Destinations__destination1__Address", user.GetEndpoint(httpLaunchProfile))
    .WithEnvironment("ReverseProxy__Clusters__catalog-cluster__Destinations__destination1__Address", catalog.GetEndpoint(httpLaunchProfile))
    .WithEnvironment("ReverseProxy__Clusters__media-cluster__Destinations__destination1__Address", media.GetEndpoint(httpLaunchProfile))
    .WithEnvironment("ReverseProxy__Clusters__order-cluster__Destinations__destination1__Address", order.GetEndpoint(httpLaunchProfile))
    .WithEnvironment("ReverseProxy__Clusters__payment-cluster__Destinations__destination1__Address", payment.GetEndpoint(httpLaunchProfile))
    .WithEnvironment("ReverseProxy__Clusters__notification-cluster__Destinations__destination1__Address", notification.GetEndpoint(httpLaunchProfile));

builder.Build().Run();
