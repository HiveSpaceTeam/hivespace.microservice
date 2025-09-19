# GitHub Copilot Instructions for HiveSpace Microservice

## Repository Overview

This repository contains the HiveSpace microservices platform built with .NET 8.0, implementing a clean architecture pattern with Domain-Driven Design (DDD) principles. The solution consists of two main services: a User Service with Identity Server integration and an API Gateway using YARP reverse proxy.

**Repository Size**: Medium-sized solution with ~10 projects  
**Languages**: C# (.NET 8.0)  
**Frameworks**: ASP.NET Core, Entity Framework Core, Duende IdentityServer, YARP  
**Target Runtime**: .NET 8.0  
**Architecture Pattern**: Clean Architecture / Onion Architecture with DDD

## Build Instructions

### Prerequisites
- .NET 8.0 SDK (minimum version 8.0.119)
- SQL Server (for User Service database)
- Visual Studio 2022 or VS Code with C# extension

### Bootstrap Commands
**ALWAYS run these commands in order from the repository root:**

1. **Restore packages** (required before any build):
   ```bash
   dotnet restore
   ```
   
2. **Build solution** (builds all projects):
   ```bash
   dotnet build
   ```

### Running the Services

**API Gateway** (no database dependencies):
```bash
cd src/HiveSpace.ApiGateway/HiveSpace.YarpApiGateway
dotnet run
```
- Starts on: https://localhost:5000
- No external dependencies

**User Service API** (requires SQL Server):
```bash
cd src/HiveSpace.UserService/HiveSpace.UserService.Api
dotnet run
```
- Starts on: https://localhost:5001
- **CRITICAL**: Requires SQL Server running on localhost:1433
- Database: UserDb
- Default connection string: `Server=localhost,1433;Database=UserDb;User Id=sa;Encrypt=False;TrustServerCertificate=True`

### Testing
- No test projects currently exist in the solution
- `dotnet test` returns immediately with no tests to run

### Build Warnings (Expected)
The build produces 6 warnings related to nullability annotations in shared domain entities. These are non-blocking:
- Nullability warnings in `HiveSpace.Domain.Shared` library
- Non-nullable property warning in `Store.cs` constructor

### Build Time
- Clean restore: ~30-45 seconds
- Build: ~15-20 seconds
- No tests to run

## Project Architecture & Layout

### Solution Structure
```
hivespace.microservice.sln         # Main solution file
Directory.Packages.props           # Centralized package management
├── src/                           # Source code
│   ├── HiveSpace.ApiGateway/
│   │   └── HiveSpace.YarpApiGateway/     # YARP reverse proxy gateway
│   └── HiveSpace.UserService/            # Identity & User management service
│       ├── HiveSpace.UserService.Api/    # Web API layer (controllers, auth)
│       ├── HiveSpace.UserService.Application/  # Application services (CQRS)
│       ├── HiveSpace.UserService.Domain/       # Domain entities & business logic
│       └── HiveSpace.UserService.Infrastructure/ # Data access & external services
└── libs/                         # Shared libraries
    ├── HiveSpace.Core/           # Core utilities & JWT handling
    ├── HiveSpace.Domain.Shared/  # Shared domain primitives (Entity, ValueObject)
    ├── HiveSpace.Application.Shared/  # Shared application patterns
    ├── HiveSpace.Infrastructure.Messaging/  # Message broker abstractions
    └── HiveSpace.Infrastructure.Persistence/  # Generic persistence patterns
```

### Key Configuration Files
- **`src/HiveSpace.UserService/HiveSpace.UserService.Api/appsettings.json`**: Main API configuration including database connection strings, IdentityServer clients, OAuth providers
- **`src/HiveSpace.ApiGateway/HiveSpace.YarpApiGateway/appsettings.json`**: YARP reverse proxy routing configuration
- **`Directory.Packages.props`**: Centralized NuGet package version management
- **`*.csproj`**: Individual project configurations with PackageReference elements

### Domain Services & Architecture
- **Clean Architecture**: Each service follows Domain > Application > Infrastructure > API layers
- **DDD Patterns**: Aggregates, Value Objects, Domain Services, Domain Events
- **Identity Management**: Duende IdentityServer with ASP.NET Core Identity
- **Database**: Entity Framework Core with SQL Server and Code-First migrations
- **API Gateway**: YARP reverse proxy with health checks and CORS support

### Key Dependencies (Pinned Versions)
- **Microsoft packages**: ASP.NET Core 8.0.11, EF Core 8.0.11, Identity 8.0.11
- **Duende IdentityServer**: 7.2.4 (requires license for production)
- **YARP**: 2.3.0
- **Serilog**: 3.1.1 for structured logging
- **FluentValidation**: 12.0.0
- **MediatR**: 13.0.0 (license required)

### Database & Seeding
- User Service uses Entity Framework migrations
- Automatic database seeding in development environment
- SeedData creates default users and roles on startup
- Migration files located in `src/HiveSpace.UserService/HiveSpace.UserService.Infrastructure/Data/Migrations/`

### Launch Profiles
Both services have multiple launch profiles in their `Properties/launchSettings.json`:
- **SelfHost**: Standard development profile
- **WSL**: WSL2 development profile

### Authentication Flow
- User Service acts as OAuth2/OIDC authorization server
- API Gateway routes requests to appropriate services
- Configured clients: Admin Portal (SPA) and API Testing Client (machine-to-machine)

## Common Development Issues & Workarounds

### Database Connection Issues
If User Service fails to start with SQL connection errors:
- Ensure SQL Server is running on localhost:1433
- Update connection string in `appsettings.json` for your environment
- In development, the service will attempt to create/migrate the database automatically

### License Warnings
Expected warnings during startup:
- Duende IdentityServer license warning (development use is allowed)
- MediatR license reminder
- These do not prevent development work

### SSL/HTTPS Warnings
Expected warnings:
- ASP.NET Core developer certificate trust warnings
- Use `dotnet dev-certs https --trust` to resolve in development

### Known TODOs in Codebase
Limited technical debt exists:
- License usage summary functionality (Program.cs)
- Message broker publishing in OutboxMessageProcessor

## Validation & CI/CD

### Manual Validation Steps
1. Build solution: `dotnet build` (should complete with 6 warnings)
2. Start API Gateway: Verify it listens on https://localhost:5000
3. If SQL Server available: Start User Service and verify IdentityServer endpoints
4. Check logs for expected license warnings only

### GitHub Workflows
- No automated CI/CD pipelines currently configured
- No `.github/workflows/` directory exists

### Code Quality
- Nullable reference types enabled across all projects
- Central package management enforced
- Clean Architecture boundaries maintained

---

## Instructions for Coding Agents

**Trust these instructions** - they are validated and current. Only search for additional information if:
- These instructions appear outdated or incorrect
- You need specific implementation details not covered here
- You encounter unexpected build or runtime errors

**Always start with**: `dotnet restore && dotnet build` before making any changes.

**For database-dependent changes**: Ensure you have SQL Server available or modify connection strings appropriately for your environment.

**When adding new projects**: Follow the existing clean architecture pattern and add entries to the main solution file.

**For API changes**: Update both the User Service (if identity-related) and API Gateway routing configurations as needed.