# .NET Microservice Templates

Custom `dotnet new` templates for creating HiveSpace microservices. Two archetypes match the repo's two service types.

## Available Templates

| Template | Short Name | Service Type | Use When |
|----------|------------|--------------|----------|
| **MsCleanArchitectureTemplate** | `ms-full` | **Full Service** | Service owns business aggregates, participates in sagas |
| **MsMinimalTemplate** | `ms-lite` | **Lite Service** | Service orchestrates infrastructure (storage, email, queues) |

For the full decision guide, see CLAUDE.md → `## Service Architecture`.

## Quick Setup

### Install Templates

```bash
dotnet new install ./MsCleanArchitectureTemplate
dotnet new install ./MsMinimalTemplate
```

### Verify Installation

```bash
dotnet new list | grep ms-
```

Expected output:
```
Full Service Template   ms-full   [C#]   Microservice/Full Service/Clean Architecture/DDD/Web API
Lite Service Template   ms-lite   [C#]   Microservice/Lite Service/Web API
```

## Creating a New Service (recommended)

Use the skill `/new-service` or the PowerShell helper directly:

```powershell
# Full Service — from repo root
.\scripts\new-service.ps1 -ServiceName HiveSpace.BillingService -TemplateName ms-full -AddToSolution

# Lite Service — from repo root
.\scripts\new-service.ps1 -ServiceName HiveSpace.SearchService -TemplateName ms-lite -AddToSolution
```

The script:
1. Runs `dotnet new` with the chosen template
2. Removes `__EMPTY_FOLDER__README.md` placeholders
3. Adds all generated `.csproj` files to `hivespace.microservice.sln` (when `-AddToSolution` is passed)

After scaffolding, follow the post-scaffold steps in `.claude/skills/new-service/SKILL.md`.

## Generated Structures

### Full Service (`ms-full`)

```
HiveSpace.[Name]Service/
├── HiveSpace.[Name]Service.Domain/
│   ├── Aggregates/
│   ├── DomainEvents/
│   ├── Enumerations/
│   ├── Exceptions/
│   │   └── [Name]DomainErrorCode.cs   ← fill in your prefix
│   ├── Repositories/
│   └── ValueObjects/
├── HiveSpace.[Name]Service.Application/
│   ├── Commands/
│   ├── Interfaces/
│   ├── Models/
│   ├── Queries/
│   ├── Services/
│   └── Validators/
├── HiveSpace.[Name]Service.Infrastructure/
│   ├── Data/
│   │   └── [Name]DbContext.cs
│   ├── DataQueries/
│   ├── EntityConfigurations/
│   ├── Identity/
│   ├── Repositories/
│   └── Services/
└── HiveSpace.[Name]Service.Api/
    ├── Controllers/
    │   └── HealthController.cs
    ├── Extensions/
    │   ├── HostingExtensions.cs
    │   └── ServiceCollectionExtensions.cs
    └── Program.cs
```

### Lite Service (`ms-lite`)

```
HiveSpace.[Name]Service/
├── HiveSpace.[Name]Service.Core/
│   ├── DomainModels/
│   ├── Exceptions/
│   │   └── [Name]DomainErrorCode.cs   ← fill in your prefix
│   ├── Infrastructure/
│   ├── Interfaces/
│   ├── Persistence/
│   │   ├── EntityConfigurations/
│   │   ├── Repositories/
│   │   └── [Name]DbContext.cs
│   ├── Services/
│   └── Validators/
└── HiveSpace.[Name]Service.Api/
    ├── Consumers/
    ├── Controllers/
    │   └── HealthController.cs
    ├── Extensions/
    │   ├── HostingExtensions.cs
    │   └── ServiceCollectionExtensions.cs
    └── Program.cs
```

## Template Management

```bash
# Reinstall after modifying template files
dotnet new uninstall ./MsCleanArchitectureTemplate
dotnet new install ./MsCleanArchitectureTemplate

dotnet new uninstall ./MsMinimalTemplate
dotnet new install ./MsMinimalTemplate
```
