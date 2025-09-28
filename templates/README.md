# .NET Microservice Templates

This folder contains custom `dotnet new` templates for creating .NET 8 microservices with different architectural patterns.

## 📋 Available Templates

| Template | Short Name | Description | Use Case |
|----------|------------|-------------|----------|
| **MsCleanArchitectureTemplate** | `microservice` | Full Clean Architecture with 4 projects | Complex microservices with business logic |
| **MsMinimalTemplate** | `minimalapi` | Lightweight Minimal API | Simple APIs and prototypes |

## 🚀 Quick Setup

### 1. Install Templates
```bash
# Install Clean Architecture template
dotnet new install ./MsCleanArchitectureTemplate

# Install Minimal API template
dotnet new install ./MsMinimalTemplate
```

### 2. Verify Installation
```bash
dotnet new list
```
You should see both templates listed:
```
Template Name                             Short Name    Language  Tags
----------------------------------------  ------------  --------  ------------------------------------------------
Clean Architecture Microservice Template  microservice  [C#]      Microservice/Clean Architecture/Projects/Web API
Minimal API Template                      minimalapi    [C#]      Minimal API/Web API/Simple
```

## 🛠️ Creating New Services

### Clean Architecture Microservice
```bash
# Create a full microservice with Clean Architecture
dotnet new microservice -n PaymentService

# Creates:
# PaymentService/
# ├── PaymentService.Api/
# ├── PaymentService.Application/
# ├── PaymentService.Domain/
# └── PaymentService.Infrastructure/
```

### Minimal API Service
```bash
# Create a lightweight API
dotnet new minimalapi -n NotificationService

# Creates:
# NotificationService/
# ├── README.md
# └── NotificationService.Api/
#     ├── NotificationService.Api.csproj
#     ├── Program.cs
#     ├── appsettings.json
#     └── appsettings.Development.json
```

## 📁 Generated Project Structures

### Clean Architecture Template
```
ServiceName/
├── ServiceName.Api/                 # Web API layer
│   ├── Controllers/
│   │   └── HealthController.cs      # Health check endpoint
│   ├── Program.cs                   # Application entry point
│   ├── appsettings.json
│   └── ServiceName.Api.csproj
├── ServiceName.Application/         # Business logic layer
│   ├── Interfaces/
│   │   └── IRepository.cs
│   ├── Services/
│   └── ServiceName.Application.csproj
├── ServiceName.Domain/              # Domain entities layer
│   ├── Entities/
│   │   └── BaseEntity.cs
│   └── ServiceName.Domain.csproj
└── ServiceName.Infrastructure/      # Data access layer
    ├── Repositories/
    │   └── Repository.cs
    └── ServiceName.Infrastructure.csproj
```

### Minimal API Template
```
ServiceName/
├── README.md                        # Project documentation
└── ServiceName.Api/                 # Single API project
    ├── ServiceName.Api.csproj
    ├── Program.cs                   # Minimal API with endpoints
    ├── appsettings.json
    └── appsettings.Development.json
```

## 🎯 When to Use Which Template

### Use **Clean Architecture** (`microservice`) when:
- ✅ Building complex business domains
- ✅ Need clear separation of concerns
- ✅ Planning for long-term maintainability
- ✅ Team development with multiple developers
- ✅ Requires extensive business logic
- ✅ Need testability and modularity

### Use **Minimal API** (`minimalapi`) when:
- ✅ Creating simple CRUD operations
- ✅ Building prototypes or proof of concepts
- ✅ Need quick API development
- ✅ Small team or solo development
- ✅ Lightweight microservices
- ✅ API gateways or proxies

## 🔧 Template Management

### List Installed Templates
```bash
dotnet new list
```

### Uninstall Templates
```bash
# Uninstall specific template
dotnet new uninstall ./MsCleanArchitectureTemplate
dotnet new uninstall ./MsMinimalTemplate

# Or uninstall all custom templates
dotnet new uninstall
```

### Reinstall After Changes
If you modify template files, you need to reinstall:
```bash
# Uninstall and reinstall
dotnet new uninstall ./MsCleanArchitectureTemplate
dotnet new install ./MsCleanArchitectureTemplate
```

## 🏗️ Project Integration

### Adding to Existing Solutions
Both templates create projects without solution files, perfect for existing solutions:

```bash
# Create the service
dotnet new microservice -n OrderService

# Add projects to your existing solution
dotnet sln add OrderService/OrderService.Api/OrderService.Api.csproj
dotnet sln add OrderService/OrderService.Application/OrderService.Application.csproj
dotnet sln add OrderService/OrderService.Domain/OrderService.Domain.csproj
dotnet sln add OrderService/OrderService.Infrastructure/OrderService.Infrastructure.csproj

# For minimal API
dotnet new minimalapi -n NotificationService
dotnet sln add NotificationService/NotificationService.Api/NotificationService.Api.csproj
```

### Recommended script (repo workspace)

You can use the included PowerShell helper to create a new service inside the repository `src/` folder. The script **does not** modify the solution by default — pass `-AddToSolution` to add generated projects to the solution automatically.

From the repository root run:

```powershell
# Create service without modifying the solution
.\scripts\new-service.ps1 -ServiceName "BillingService" -TemplateName "clean-architecture-ms"

# Create service and add projects to the solution
.\scripts\new-service.ps1 -ServiceName "BillingService" -TemplateName "clean-architecture-ms" -AddToSolution
```

- Notes:
- `-TemplateName` should match the template short name from the template's `template.json`.
- Current template short names in this repo:
    - Clean Architecture: `clean-architecture-ms` (found in `MsCleanArchitectureTemplate/.template.config/template.json`)
    - Minimal API: `minimalapi` (found in `MsMinimalTemplate/.template.config/template.json`)
- You can list installed templates with `dotnet new list` to see available short names on your machine.
- If the template short name is not installed, the script will try to find and install a matching local template under `./templates`.
- The script removes placeholder files used for empty folders (`__EMPTY_FOLDER__README.md`).


### Building and Running
```bash
# Clean Architecture
cd PaymentService/PaymentService.Api
dotnet run

# Minimal API
cd NotificationService/NotificationService.Api
dotnet run
```

## 🧪 Testing the Templates

### Quick Test Setup
```bash
# Create test directory
mkdir template-test
cd template-test

# Test both templates
dotnet new microservice -n TestMicroservice
dotnet new minimalapi -n TestNotification

# Verify structures
ls -la
```

### Verify Generated Code
- ✅ Check project references are correct
- ✅ Verify namespace consistency
- ✅ Test build and run
- ✅ Access Swagger UI at `/swagger`
- ✅ Test health endpoints at `/health` or `/api/health`

## 📚 API Documentation

Both templates include Swagger/OpenAPI documentation:
- **Clean Architecture**: `https://localhost:5001/swagger`
- **Minimal API**: `https://localhost:5001/swagger`

## 🔍 Health Checks

Both templates include health check endpoints:
- **Clean Architecture**: `GET /api/health`
- **Minimal API**: `GET /health`

## 🆘 Troubleshooting

### Template Not Found
```bash
# Make sure templates are installed
dotnet new list | grep -E "(microservice|minimalapi)"
```

### Permission Issues
```bash
# Run as administrator if needed
# Or use full paths in commands
```

### Build Errors
```bash
# Clean and restore
dotnet clean
dotnet restore
dotnet build
```

### Template Cache Issues
```bash
# Clear template cache
dotnet new uninstall
dotnet new install [template-path]
```

## 🤝 Contributing

To modify templates:
1. Edit template files in their respective folders
2. Update `template.json` configuration if needed
3. Uninstall and reinstall the template
4. Test the changes with new project creation
5. Update this README if adding new features

## 📋 Template Configuration Files

- **MsCleanArchitectureTemplate**: `.template.config/template.json`
- **MsMinimalTemplate**: `.template.config/template.json`

Key configuration properties:
- `sourceName`: Parameter name (ServiceName)
- `shortName`: CLI command name
- `preferNameDirectory`: Folder creation behavior
- `exclude`: Files/folders to ignore during generation

---

**Happy coding! 🚀**

> These templates help standardize microservice creation across teams while providing flexibility for different architectural needs.