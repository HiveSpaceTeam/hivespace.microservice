# HiveSpace Backend Runtime

## Prerequisites

- .NET SDK 8.0.411 or a compatible later .NET 8 feature band. The repo root `global.json` is the SDK source of truth.
- Docker Desktop or another Docker-compatible runtime for local infrastructure containers.
- Aspire CLI for `aspire run`.
- Azure Functions Core Tools for the MediaService Function resource.

## Run With .NET

From the repository root:

```powershell
dotnet restore
dotnet build
dotnet run --project .\src\HiveSpace.AppHost\HiveSpace.AppHost.csproj
```

The AppHost is the default backend local runtime and replaces Docker Compose for backend local development. It starts the gateway, backend API services, MediaService Function, and required local infrastructure together.

## Run With Aspire CLI

From the repository root:

```powershell
aspire run
```

The Aspire CLI uses `aspire.config.json`, which points at `src/HiveSpace.AppHost/HiveSpace.AppHost.csproj`.

To run in the background:

```powershell
aspire run --detach
```

Frontend dev servers are outside AppHost v1. Frontend apps should keep using the existing gateway base URL:

```text
http://localhost:5000
```

## Fixed Local Ports

| Resource | Port |
| --- | ---: |
| ApiGateway | 5000 |
| IdentityService | 5001 |
| CatalogService | 5002 |
| MediaService API | 5003 |
| MediaService Function | 7072 |
| OrderService | 5004 |
| PaymentService | 5005 |
| NotificationService | 5006 |
| UserService | 5007 |
| SQL Server | 1433 |
| RabbitMQ AMQP | 5672 |
| Kafka | 9092 |
| Redis | 6379 |
| Azurite Blob | 10000 |
| Azurite Queue | 10001 |
| Azurite Table | 10002 |

## Runtime View

When AppHost starts, use the Aspire dashboard URL printed by the process to inspect service/dependency status, logs, traces, and metrics. Logs remain diagnostic output; health/status and OpenTelemetry signals are the primary local runtime view.

## Configuration Contract

Dependency endpoints and secrets use compact lowercase `ConnectionStrings` keys, including service databases, `rabbitmq`, `kafka`, `redis`, `azureservicebus`, and `azurestorage`.

Broker selection remains under `Messaging`:

```json
{
  "Messaging": {
    "EnableRabbitMq": true,
    "EnableKafka": false,
    "EnableAzureServiceBus": false
  }
}
```

Keep nested `Messaging` provider sections for non-secret tuning only. If a broker flag is enabled and the required connection string is missing, startup fails with a clear configuration error.

## Stop And Restart

Stop foreground `dotnet run` or `aspire run` sessions with `Ctrl+C` in the terminal that started them.

For detached/background sessions, stop the AppHost process and any remaining Azure Functions Core Tools child process:

```powershell
Get-Process HiveSpace.AppHost,func -ErrorAction SilentlyContinue | Stop-Process
```

AppHost-managed local data is intended to survive normal stop/restart where Aspire persistent resource support is configured. SQL Server, RabbitMQ, Kafka, Redis, and Azurite containers may continue running after AppHost exits because they are configured with persistent lifetimes and restart policies. Existing Docker Compose local data is not migrated. If Redis was previously started with a generated password, reset the old Redis container or volume once so it uses the stable AppHost Redis password.

## MediaService Function

The MediaService Function runs as the `media-func` Aspire resource via Azure Functions Core Tools. It uses Azurite host storage, the same `mediadb` database reference as MediaService API, RabbitMQ for messaging, and Azurite blob/queue resources for media storage and queue triggers.
