# HiveSpace User Service

> **Identity & User Management Service** - A comprehensive user management service with Duende IdentityServer integration for OAuth2/OIDC authentication and authorization.

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- SQL Server (localhost:1433 by default)
- Visual Studio 2022 or VS Code with C# extension

### Running the Service
```bash
# From the API project directory
cd src/HiveSpace.UserService/HiveSpace.UserService.Api
dotnet run
```

**Service URL**: https://localhost:5001  
**Identity Server Endpoints**: https://localhost:5001/.well-known/openid_configuration

### Database Setup
The service automatically creates and seeds the database on startup in development mode. Default connection string:
```
Server=localhost,1433;Database=UserDb;User Id=sa;Password={YourPassword};Encrypt=False;TrustServerCertificate=True
```

## 📋 Table of Contents
- [Architecture Overview](#-architecture-overview)
- [Project Structure](#-project-structure)
- [Domain Design](#-domain-design)
- [Identity Server Integration](#-identity-server-integration)
- [Data Access Patterns](#-data-access-patterns)
- [API Endpoints](#-api-endpoints)
- [Development Guidelines](#-development-guidelines)
- [Adding New Features](#-adding-new-features)
- [Configuration](#-configuration)
- [Testing](#-testing)

## 🏗️ Architecture Overview

The User Service follows **Clean Architecture** principles with **Domain-Driven Design (DDD)** patterns:

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                            │
│  Controllers, Authentication, Authorization, Validation     │
├─────────────────────────────────────────────────────────────┤
│                   Application Layer                         │
│     Services, DTOs, Interfaces, Command/Query Handlers      │
├─────────────────────────────────────────────────────────────┤
│                     Domain Layer                            │
│   Entities, Value Objects, Domain Services, Repositories    │
├─────────────────────────────────────────────────────────────┤
│                 Infrastructure Layer                        │
│  Data Access, Identity Provider, External Services, EF Core │
└─────────────────────────────────────────────────────────────┘
```

### Key Architectural Decisions

1. **Clean Architecture Boundaries**: Strict dependency inversion with domain at the center
2. **DDD Patterns**: Aggregates, Value Objects, Domain Services for business logic
3. **CQRS-Ready**: Separated read and write operations (queries can bypass domain)
4. **Identity Integration**: Duende IdentityServer with ASP.NET Core Identity
5. **Database-First Approach**: Entity Framework Core with Code-First migrations

## 📁 Project Structure

```
HiveSpace.UserService/
├── HiveSpace.UserService.Api/              # 🌐 Web API Layer
│   ├── Controllers/                        # API controllers
│   ├── Extensions/                         # Service registrations & pipeline config
│   ├── Pages/                             # IdentityServer UI pages
│   └── Program.cs                         # Application entry point
│
├── HiveSpace.UserService.Application/      # 🔧 Application Layer
│   ├── Services/                          # Application services (orchestration)
│   ├── Models/                            # DTOs, requests, responses
│   ├── Interfaces/                        # Application contracts
│   └── Validators/                        # FluentValidation rules
│
├── HiveSpace.UserService.Domain/           # 🏛️ Domain Layer
│   ├── Aggregates/                        # Domain entities & aggregates
│   │   ├── User/                          # User aggregate root
│   │   └── Store/                         # Store aggregate root
│   ├── Services/                          # Domain services (business logic)
│   ├── Repositories/                      # Repository interfaces
│   └── Exceptions/                        # Domain-specific exceptions
│
└── HiveSpace.UserService.Infrastructure/   # 🔌 Infrastructure Layer
    ├── Data/                              # DbContext & migrations
    ├── Identity/                          # Identity Server integration
    ├── Repositories/                      # Repository implementations
    ├── DataQueries/                       # Direct database queries (CQRS read side)
    └── Mappers/                           # Domain ↔ Infrastructure mappings
```

## 🏛️ Domain Design

### Core Aggregates

#### User Aggregate
**Root Entity**: `User`
- **Value Objects**: `Email`, `PhoneNumber`, `DateOfBirth`, `Address`
- **Enums**: `UserStatus`, `Gender`, `Role`
- **Business Rules**: Email uniqueness, password requirements, status transitions

```csharp
// Example: Creating a new user through domain service
var user = await userManager.RegisterUserAsync(
    Email.Create("user@example.com"),
    "username",
    "John Doe",
    Role.Customer,
    phoneNumber: PhoneNumber.Create("+1234567890")
);
```

#### Store Aggregate
**Root Entity**: `Store`
- **Business Rules**: Store ownership, user assignments

### Domain Services
- **`UserManager`**: User registration, email validation, business rules
- **`StoreManager`**: Store management, user assignments

### Repository Pattern
- **`IUserRepository`**: Domain-focused user operations
- **`IStoreRepository`**: Store management operations

## 🔐 Identity Server Integration

The service integrates **Duende IdentityServer v7** for OAuth2/OIDC authentication and authorization. This provides enterprise-grade identity management with support for various authentication flows.

### 🚀 Quick Setup Guide

#### 1. **License Configuration**
  "ClientId": "adminportal",
  "ClientName": "Admin Portal",
  "GrantTypes": ["authorization_code"],
  "RequirePkce": true,
  "RequireClientSecret": false,
  "AllowOfflineAccess": false,
  "RedirectUris": [
    "http://localhost:5173/callback",
    "http://localhost:5173/callback.html",
    "http://localhost:5173/silent-renew.html"
  ],
  "PostLogoutRedirectUris": ["http://localhost:5173/"],
  "AllowedCorsOrigins": ["http://localhost:5173"],
  "AllowedScopes": ["openid", "profile", "user.fullaccess"],
  "AccessTokenLifetime": 7200,
  "IdentityTokenLifetime": 7200
}
```

**Use Case**: Frontend applications (React, Angular, Vue.js)  
**Flow**: Authorization Code + PKCE (most secure for SPAs)

#### 2. **API Testing Client** (Machine-to-Machine)
```json
{
  "ClientId": "apitestingapp",
  "ClientName": "API Testing Client",
  "GrantTypes": ["client_credentials"],
  "RequireClientSecret": true,
  "ClientSecret": "apitestingsecret",
  "AllowedScopes": ["user.fullaccess"]
}
```

**Use Case**: Backend services, API testing tools (Postman, Insomnia)  
**Flow**: Client Credentials (no user interaction required)

### 🔑 Available Scopes & Resources

#### **Identity Resources** (User Information)
- **`openid`**: Required for OpenID Connect
- **`profile`**: User profile claims (name, email, etc.)

#### **API Scopes** (Resource Access)
- **`user.fullaccess`**: Complete user management operations
- **`order.fullaccess`**: Order service operations (future)
- **`basket.fullaccess`**: Shopping basket operations (future)
- **`catalog.fullaccess`**: Product catalog operations (future)
- **`hivespace-backend.fullaccess`**: Backend API access (future)

### 🌐 External Authentication Providers

#### **Google OAuth Setup**
```json
// appsettings.json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id.apps.googleusercontent.com",
      "ClientSecret": "your-google-client-secret"
    }
  }
}
```

**Getting Google Credentials:**
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing
3. Enable Google+ API
4. Create OAuth 2.0 credentials
5. Add authorized redirect URIs:
   - `https://localhost:5001/signin-google`
   - `http://localhost:5001/signin-google` (for development)

#### **Adding More Providers**
```csharp
// In HostingExtensions.cs
services.AddAuthentication()
    .AddGoogle(options => { /* Google config */ })
    .AddMicrosoftAccount(options => { /* Microsoft config */ })
    .AddFacebook(options => { /* Facebook config */ });
```

### ⚙️ Adding New Clients

#### **Step 1: Configure in appsettings.json**
```json
{
  "Clients": {
    "my-new-client": {
      "ClientId": "my-new-client",
      "ClientName": "My New Application",
      "ClientUri": "https://myapp.com",
      "RequireClientSecret": true,
      "ClientSecret": "my-secret-key",
      "AllowedGrantTypes": ["authorization_code"],
      "RequirePkce": true,
      "RequireConsent": false,
      "AllowOfflineAccess": true,
      "RedirectUris": [
        "https://myapp.com/callback"
      ],
      "PostLogoutRedirectUris": [
        "https://myapp.com/logout"
      ],
      "AllowedCorsOrigins": [
        "https://myapp.com"
      ],
      "AllowedScopes": [
        "openid",
        "profile",
        "user.fullaccess"
      ],
      "AccessTokenLifetime": 3600,
      "IdentityTokenLifetime": 3600
    }
  }
}
```

#### **Step 2: Client Configuration Reference**

| Property | Description | Example Values |
|----------|-------------|----------------|
| `ClientId` | Unique identifier | `"mobile-app"`, `"web-portal"` |
| `GrantTypes` | OAuth2 flow types | `["authorization_code"]`, `["client_credentials"]` |
| `RequirePkce` | PKCE requirement (recommended for SPAs) | `true` |
| `RequireClientSecret` | Secret requirement | `false` (SPA), `true` (server apps) |
| `AllowOfflineAccess` | Refresh token support | `true` for long-lived sessions |
| `AccessTokenLifetime` | Token validity (seconds) | `3600` (1 hour) |
| `AllowedScopes` | Permitted access scopes | `["openid", "profile", "api.read"]` |

### 🔗 Identity Server Endpoints

#### **Standard OIDC Endpoints**
- **Discovery**: `/.well-known/openid_configuration`
- **Authorization**: `/connect/authorize`
- **Token**: `/connect/token`
- **UserInfo**: `/connect/userinfo`
- **Token Introspection**: `/connect/introspect`
- **Token Revocation**: `/connect/revocation`
- **End Session**: `/connect/endsession`

#### **Admin UI Pages** (Built-in)
- **Login**: `/Account/Login`
- **Logout**: `/Account/Logout`
- **Consent**: `/Consent`
- **Error**: `/Home/Error`
- **Grants**: `/Grants` (user consent management)

### 🧪 Testing Your Configuration

#### **1. Test Discovery Endpoint**
```bash
curl https://localhost:5001/.well-known/openid_configuration
```

#### **2. Test Client Credentials Flow** (Machine-to-Machine)
```bash
curl -X POST https://localhost:5001/connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=apitestingapp&client_secret=apitestingsecret&scope=user.fullaccess"
```

#### **3. Test Authorization Code Flow** (SPA)
1. Navigate to: `https://localhost:5001/connect/authorize?client_id=adminportal&response_type=code&scope=openid%20profile%20user.fullaccess&redirect_uri=http://localhost:5173/callback&code_challenge=your-code-challenge&code_challenge_method=S256`
2. Login with test user
3. Get authorization code in callback

#### **4. Using Postman**
1. Create new OAuth 2.0 request
2. **Authorization URL**: `https://localhost:5001/connect/authorize`
3. **Access Token URL**: `https://localhost:5001/connect/token`
4. **Client ID**: `adminportal`
5. **Scope**: `openid profile user.fullaccess`

### 🔧 Development Users

The service automatically seeds test users in development:

| Email | Password | Role |
|-------|----------|------|
| `admin@hivespace.com` | `Admin123!` | Admin |
| `user@hivespace.com` | `User123!` | Customer |
| `storeowner@hivespace.com` | `Store123!` | StoreOwner |

### 🚨 Common Issues & Solutions

#### **1. "Invalid redirect_uri" Error**
```json
// Ensure exact match in appsettings.json
"RedirectUris": [
  "http://localhost:3000/callback",  // ❌ Wrong port
  "http://localhost:5173/callback"   // ✅ Correct
]
```

#### **2. CORS Issues**
```json
// Add client's origin to CORS
"AllowedCorsOrigins": [
  "http://localhost:5173",
  "https://myapp.com"
]
```

#### **3. "Invalid scope" Error**
```json
// Ensure scope exists in ApiScopes configuration
"AllowedScopes": [
  "openid",
  "profile",
  "user.fullaccess"  // Must exist in Config.cs ApiScopes
]
```

#### **4. Token Lifetime Issues**
```json
// Adjust token lifetimes as needed
"AccessTokenLifetime": 7200,      // 2 hours
"IdentityTokenLifetime": 7200,    // 2 hours
"RefreshTokenLifetime": 2592000   // 30 days
```

### 📚 Useful Resources

- **[Duende IdentityServer Docs](https://docs.duendesoftware.com/identityserver/v7)**
- **[OAuth 2.0 Flows Guide](https://auth0.com/docs/get-started/authentication-and-authorization-flow)**
- **[OpenID Connect Specification](https://openid.net/connect/)**
- **[PKCE RFC](https://tools.ietf.org/html/rfc7636)** (for SPA security)

### 🎯 Next Steps for New Team Members

1. **Understand the flows**: Review OAuth2/OIDC concepts
2. **Test with existing clients**: Use Postman or the admin portal
3. **Create your own client**: Follow the configuration guide above
4. **Explore the UI**: Check out the built-in Identity Server pages
5. **Read the logs**: Enable debug logging to understand token flows

## 🗃️ Data Access Patterns

The service implements **two distinct data access patterns** to support different use cases:

### 1. Repository Pattern (Write Operations)
**Use Case**: Domain operations that require business logic validation

```csharp
public class AdminService : IAdminService
{
    private readonly UserManager _domainUserManager;
    private readonly IUserRepository _userRepository;

    public async Task<CreateAdminResponseDto> CreateAdminAsync(CreateAdminRequestDto request)
    {
        // Goes through domain validation and business rules
        var user = await _domainUserManager.RegisterUserAsync(
            Email.Create(request.Email),
            request.UserName,
            request.FullName,
            Role.Admin
        );
        
        await _userRepository.SaveChangesAsync();
        return user.MapToResponse();
    }
}
```

**Flow**: Controller → Application Service → Domain Service → Repository → Database

### 2. Direct Query Pattern (Read Operations)
**Use Case**: Data retrieval operations that can bypass domain logic for performance

```csharp
public class AdminService : IAdminService
{
  private readonly IAdminDataQuery _adminDataQuery;

  public async Task<GetAdminResponseDto> GetAdminsAsync(GetAdminRequestDto request)
  {
    // Bypasses domain layer for read-only admin retrieval
    var admins = await _adminDataQuery.GetPagingAdminsAsync(
      new AdminUserFilterRequest
      {
        SearchTerm = request.SearchTerm,
        Status = request.Status, // StatusFilter enum
        Role = request.Role,     // RoleFilter enum
        PageNumber = request.PageNumber,
        PageSize = request.PageSize
      }
    );

    var total = await _adminDataQuery.GetTotalAdminsCountAsync(
      new AdminUserFilterRequest
      {
        SearchTerm = request.SearchTerm,
        Status = request.Status,
        Role = request.Role
      }
    );

    return new GetAdminResponseDto { Items = admins, Total = total };
  }
}
```
}
```

**Flow**: Controller → Application Service → Data Query → Database (Direct SQL)

### When to Use Each Pattern

| Pattern | Use Case | Examples |
|---------|----------|----------|
| **Repository** | Write operations, business rules | Create user, Update profile, Delete account |
| **Direct Query** | Read operations, reporting, search | User lists, search, dashboards, analytics |

## 🌐 API Endpoints

### Authentication Required
All endpoints require authentication with the `user.fullaccess` scope.

### Admin Controller (`/api/v1/admins`)

#### Create Admin
```http
POST /api/v1/admins
Authorization: Bearer {token}
Content-Type: application/json

{
  "email": "admin@example.com",
  "userName": "adminuser",
  "fullName": "Admin User",
  "phoneNumber": "+1234567890"
}
```

#### Get Admins (Paginated)
```http
GET /api/v1/admins?searchTerm=john&status=Active&role=Admin&pageNumber=1&pageSize=10
Authorization: Bearer {token}
```

**Query Parameters:**
- `searchTerm` (optional): Search in name, email, username
- `status` (optional): Admin status filter (`StatusFilter` enum; e.g., `Active`, `Inactive`, `Suspended`)
- `role` (optional): Admin role filter (`RoleFilter` enum; e.g., `Admin`, `SuperAdmin`)
- `pageNumber` (default: 1): Page number for pagination
- `pageSize` (default: 10): Items per page

### API Versioning
- **Current Version**: `v1.0`
- **URL**: `/api/v{version:apiVersion}/`


## 🛠️ Development Guidelines

### Adding New Features

#### 1. For Write Operations (Command/Domain Logic)
```csharp
// Step 1: Add to Domain Layer
public class User : AggregateRoot<Guid>
{
    public void UpdateProfile(string fullName, PhoneNumber? phoneNumber)
    {
        // Domain validation logic here
        FullName = fullName;
        PhoneNumber = phoneNumber;
        RaiseDomainEvent(new UserProfileUpdatedEvent(Id));
    }
}

// Step 2: Add to Application Layer
public interface IUserService
{
    Task<UpdateProfileResponseDto> UpdateProfileAsync(UpdateProfileRequestDto request);
}

// Step 3: Add to API Layer
[HttpPut("{id}/profile")]
public async Task<ActionResult> UpdateProfile(Guid id, UpdateProfileRequestDto request)
{
    var result = await _userService.UpdateProfileAsync(request);
    return Ok(result);
}
```

#### 2. For Read Operations (Query/Reporting)
```csharp
// Step 1: Add to Infrastructure DataQueries
public interface IUserDataQuery
{
    Task<UserProfileDto> GetUserProfileAsync(Guid userId);
}

// Step 2: Add to Application Service
public async Task<UserProfileDto> GetUserProfileAsync(Guid userId)
{
    return await _userDataQuery.GetUserProfileAsync(userId);
}

// Step 3: Add to API Controller
[HttpGet("{id}/profile")]
public async Task<ActionResult<UserProfileDto>> GetProfile(Guid id)
{
    var profile = await _userService.GetUserProfileAsync(id);
    return Ok(profile);
}
```

### Code Style Guidelines

#### Domain Layer
- **Aggregates**: Business logic, invariants, domain events
- **Value Objects**: Immutable, self-validating objects
- **Domain Services**: Cross-aggregate business operations
- **Repository Interfaces**: Data access contracts

#### Application Layer
- **Services**: Orchestration, DTO mapping, transaction coordination
- **DTOs**: Data transfer objects for API communication
- **Validators**: FluentValidation rules for input validation

#### Infrastructure Layer
- **Repositories**: Domain repository implementations
- **DataQueries**: Direct database access for read operations
- **Mappers**: Convert between domain and infrastructure models

### Validation Strategy
```csharp
// FluentValidation in Application layer
public class CreateAdminValidator : AbstractValidator<CreateAdminRequestDto>
{
    public CreateAdminValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
            
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MinimumLength(2);
    }
}

// Domain validation in Domain layer
public static Email Create(string value)
{
    if (string.IsNullOrWhiteSpace(value))
        throw new DomainException("Email cannot be empty");
        
    if (!IsValidEmailFormat(value))
        throw new DomainException("Invalid email format");
        
    return new Email(value);
}
```

## ⚙️ Configuration

### Key Configuration Sections

#### Connection Strings
```json
{
  "ConnectionStrings": {
    "UserServiceDb": "Server=localhost,1433;Database=UserDb;User Id=sa;Password=Passw0rd123!;Encrypt=False;TrustServerCertificate=True"
  }
}
```

#### Identity Server Clients
```json
{
  "Clients": {
    "adminportal": {
      "ClientId": "adminportal",
      "AllowedGrantTypes": ["authorization_code"],
      "RedirectUris": ["http://localhost:5173/callback"],
      "AllowedScopes": ["openid", "profile", "user.fullaccess"]
    }
  }
}
```

#### External Authentication
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    }
  }
}
```

### Environment-Specific Settings
- **Development**: Automatic database seeding, detailed logging
- **Production**: Secure connection strings, optimized logging

## 🧪 Testing

### Test Structure (Planned)
```
test/
├── HiveSpace.UserService.UnitTests/        # Domain & Application unit tests
├── HiveSpace.UserService.IntegrationTests/ # Infrastructure & API integration tests
└── HiveSpace.UserService.FunctionalTests/  # End-to-end API tests
```

### Testing Guidelines
- **Unit Tests**: Domain logic, application services, validators
- **Integration Tests**: Database operations, external service integrations
- **Functional Tests**: API endpoints, authentication flows

### Current Status
⚠️ **Test projects are not implemented yet.** This is a priority item for the development roadmap.

## 🔄 Database Migrations

### Running Migrations
```bash
# From Infrastructure project
cd src/HiveSpace.UserService/HiveSpace.UserService.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../HiveSpace.UserService.Api
dotnet ef database update --startup-project ../HiveSpace.UserService.Api
```

### Seed Data
Development environment automatically seeds:
- **Default Admin User**: admin@hivespace.com / Admin123!
- **Sample Customer Users**: Various test users with different roles
- **Default Roles**: Admin, Customer, StoreOwner

## 🚨 Common Issues & Solutions

### SQL Server Connection Issues
```bash
# Check SQL Server is running
sqlcmd -S localhost,1433 -U sa -P Passw0rd123!

# Update connection string in appsettings.json if needed
```

### License Warnings (Expected)
- **Duende IdentityServer**: Community license warnings (safe for development)
- **MediatR**: License reminder (safe for development)

### HTTPS Certificate Issues
```bash
# Trust development certificates
dotnet dev-certs https --trust
```

## 📚 Additional Resources

- [Duende IdentityServer Documentation](https://docs.duendesoftware.com/identityserver/v7)
- [Clean Architecture Pattern](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/tags/domain%20driven%20design.html)
- [ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)

## 🤝 Contributing

### Development Workflow
1. **Understand the Architecture**: Review this README and examine existing code
2. **Follow Patterns**: Use established patterns for new features
3. **Write Tests**: Add appropriate test coverage (when test infrastructure is available)
4. **Validate Changes**: Ensure builds pass and functionality works
5. **Update Documentation**: Keep this README updated with changes

### Code Review Checklist
- [ ] Follows Clean Architecture boundaries
- [ ] Domain logic is in the domain layer
- [ ] Appropriate data access pattern (Repository vs Direct Query)
- [ ] Input validation is implemented
- [ ] Authentication/Authorization is applied
- [ ] Error handling is consistent
- [ ] Tests are written (when available)

---

## 📝 License

This project is part of the HiveSpace microservices platform. Please refer to the main repository license for details.

**Last Updated**: September 2025  
**Service Version**: 1.0  
**Framework**: .NET 8.0
