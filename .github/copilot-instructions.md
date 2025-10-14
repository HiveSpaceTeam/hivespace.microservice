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

## Development Workflow for DDD Services

### For Command Operations & Simple Query Operations (Domain-First Approach)

When implementing **write operations** or **simple read operations** that require domain validation, follow this sequence:

#### 1. **Domain Layer** (Start Here)
```csharp
// Example from User aggregate - Domain entity with business logic
public class User : AggregateRoot<Guid>, IAuditable
{
    public Email Email { get; private set; }
    public string FullName { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    
    // Business method with domain validation
    public void UpdateProfile(string fullName, PhoneNumber? phoneNumber)
    {
        ValidateFullName(fullName);
        FullName = fullName;
        PhoneNumber = phoneNumber;
        RaiseDomainEvent(new UserProfileUpdatedEvent(Id));
    }
}

// Example from UserManager - Domain service for complex business operations
public class UserManager : IDomainService
{
    private readonly IUserRepository _userRepository;
    
    public async Task<User> RegisterUserAsync(Email email, string userName, string fullName, 
        CancellationToken cancellationToken = default)
    {
        // Domain validation and business rules
        await CanUserBeRegisteredAsync(email, userName?.Trim() ?? string.Empty, cancellationToken);
        var user = User.Create(email, userName?.Trim() ?? string.Empty, PasswordPlaceholder, fullName);
        return user;
    }
    
    private async Task<bool> CanUserBeRegisteredAsync(Email email, string userName, CancellationToken cancellationToken)
    {
        if (!await IsEmailAvailableAsync(email, cancellationToken))
            throw new ConflictException(UserDomainErrorCode.EmailAlreadyExists, nameof(User.Email));
        if (!await IsUserNameAvailableAsync(userName, cancellationToken))
            throw new ConflictException(UserDomainErrorCode.UserNameAlreadyExists, nameof(User.UserName));
        return true;
    }
}

// Repository interface in domain layer
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<User> CreateUserAsync(User domainUser, string password, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

#### 2. **Application Layer**
```csharp
// Example from CreateAdminRequestDto - Request DTO
public record CreateAdminRequestDto(
    string Email,
    string Password,
    string FullName,
    string ConfirmPassword,
    bool IsSystemAdmin = false
);

// Example from CreateAdminResponseDto - Response DTO
public record CreateAdminResponseDto(
    Guid Id,
    string Email,
    string FullName,
    bool IsSystemAdmin,
    DateTimeOffset CreatedAt,
    bool IsActive = true
);

// Example from CreateAdminValidator - FluentValidation validator
public class CreateAdminValidator : AbstractValidator<CreateAdminRequestDto>
{
    public CreateAdminValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateAdminRequestDto.FullName)))
            .Length(2, 100)
            .WithState(_ => new Error(UserDomainErrorCode.InvalidField, nameof(CreateAdminRequestDto.FullName)));

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithState(_ => new Error(CommonErrorCode.Required, nameof(CreateAdminRequestDto.Email)))
            .EmailAddress()
            .WithState(_ => new Error(UserDomainErrorCode.InvalidEmail, nameof(CreateAdminRequestDto.Email)));

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(12)
            .Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]+$");
    }
}

// Example from AdminService - Application service orchestrating domain operations
public class AdminService : IAdminService
{
    private readonly IUserContext _userContext;
    private readonly UserManager _domainUserManager;
    private readonly IUserRepository _userRepository;

    public async Task<CreateAdminResponseDto> CreateAdminAsync(CreateAdminRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        // Build domain value objects
        var email = Email.Create(request.Email);
        var role = Role.FromName(request.IsSystemAdmin ? Role.RoleNames.SystemAdmin : Role.RoleNames.Admin);

        // Use domain manager for business logic validation
        var domainUser = await _domainUserManager.CreateAdminUserAsync(
            email, email, request.FullName.Trim(), role, _userContext.UserId, cancellationToken);

        // Persist through repository
        var created = await _userRepository.CreateUserAsync(domainUser, request.Password, cancellationToken);

        // Map to response DTO
        return new CreateAdminResponseDto(
            created.Id, created.Email.Value, created.FullName, 
            created.Role?.Name == Role.RoleNames.SystemAdmin, created.CreatedAt);
    }
}
```

#### 3. **Register in DI Container**
```csharp
// Example from ServiceCollectionExtensions.cs - Actual DI registration
public static void AddAppApplicationServices(this IServiceCollection services)
{
    services.AddScoped<IUserContext, UserContext>();
    services.AddScoped<IAdminService, AdminService>();
}

public static void AddAppDomainServices(this IServiceCollection services)
{
    services.AddScoped<UserManager>();
    services.AddScoped<StoreManager>();
}
```

#### 4. **Infrastructure Layer**
```csharp
// Example from UserRepository - Repository implementation
public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public async Task<User> CreateUserAsync(User domainUser, string password, CancellationToken cancellationToken = default)
    {
        var appUser = domainUser.ToApplicationUser();
        var result = await _userManager.CreateAsync(appUser, password);
        
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        // Add to role if specified
        if (domainUser.Role != null)
            await _userManager.AddToRoleAsync(appUser, domainUser.Role.Name);

        await _context.SaveChangesAsync(cancellationToken);
        
        // Convert back to domain entity
        var roleNames = await _userManager.GetRolesAsync(appUser);
        return appUser.ToDomainUser(roleNames);
    }
}

// Entity configuration example
public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
    }
}
```

#### 5. **API Layer**
```csharp
// Example from AdminController - Actual controller implementation
[ApiController]
[Route("api/v{version:apiVersion}/admins")]
[ApiVersion("1.0")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService) => _adminService = adminService;

    [HttpPost]
    [Authorize(Policy = "RequireUserFullAccessScope")]
    public async Task<ActionResult<CreateAdminResponseDto>> CreateAdmin(
        [FromBody] CreateAdminRequestDto request, CancellationToken cancellationToken)
    {
        // Validate request using FluentValidation
        ValidationHelper.ValidateResult(new CreateAdminValidator().Validate(request));
        
        // Call application service
        var result = await _adminService.CreateAdminAsync(request, cancellationToken);
        
        return CreatedAtAction(nameof(CreateAdmin), new { id = result.Id }, result);
    }
}
```

### For Complex Query Operations (Bypass Domain Approach)

When implementing **complex read operations** that don't require domain validation and need performance optimization:

#### 1. **Application Layer**
```csharp
// Example request DTO for complex queries
public record GetUsersRequestDto(
    string? SearchTerm = null,
    UserStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 10,
    string? SortField = null,
    SortDirection? SortDirection = null
);

// Example response DTO
public record GetUsersResponseDto(
    PagedResult<UserListItemDto> Users
);

public record UserListItemDto(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    UserStatus Status,
    bool IsSeller,
    DateTime CreatedDate,
    DateTime? LastLoginDate,
    string Avatar
);

// Validator for query request
public class GetUsersValidator : AbstractValidator<GetUsersRequestDto>
{
    public GetUsersValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

// Application service using data query
public class AdminService : IAdminService
{
    private readonly IUserDataQuery _userDataQuery;

    public async Task<GetUsersResponseDto> GetUsersAsync(GetUsersRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        // Convert to internal query model
        var filterRequest = new AdminUserFilterRequest
        {
            SearchTerm = request.SearchTerm,
            Status = request.Status,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortField = request.SortField ?? "CreatedDate",
            SortDirection = request.SortDirection ?? SortDirection.Desc
        };

        // Execute direct query bypassing domain
        var result = await _userDataQuery.GetPagingUsersAsync(filterRequest, cancellationToken);
        
        return new GetUsersResponseDto(result);
    }
}
```

#### 2. **Data Query Interface**
```csharp
// Example from IUserDataQuery - Query interface in Application layer
public interface IUserDataQuery
{
    Task<PagedResult<UserListItemDto>> GetPagingUsersAsync(AdminUserFilterRequest request, 
        CancellationToken cancellationToken = default);
    Task<UserListItemDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
```

#### 3. **Infrastructure Layer (DataQueries)**
```csharp
// Example from UserDataQuery - Direct SQL implementation
public class UserDataQuery : IUserDataQuery
{
    private readonly string _connectionString;

    public UserDataQuery(string connectionString) => 
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

    public async Task<PagedResult<UserListItemDto>> GetPagingUsersAsync(AdminUserFilterRequest request, 
        CancellationToken cancellationToken = default)
    {
        var whereConditions = BuildWhereConditions(request);
        var orderBy = BuildOrderByClause(request.SortField, request.SortDirection);

        // Complex SQL query for performance
        var mainQuery = $@"
            WITH FilteredUsers AS (
                SELECT DISTINCT
                    u.Id,
                    u.UserName AS Username,
                    u.FullName,
                    u.Email,
                    u.Status,
                    CAST(CASE WHEN u.StoreId IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsSeller,
                    CAST(u.CreatedAt AT TIME ZONE 'UTC' AS DATETIME2) AS CreatedDate,
                    CAST(u.LastLoginAt AT TIME ZONE 'UTC' AS DATETIME2) AS LastLoginDate,
                    '' AS Avatar
                FROM users u
                WHERE NOT EXISTS (
                    SELECT 1 FROM user_roles ur2 JOIN roles r2 ON ur2.RoleId = r2.Id 
                    WHERE ur2.UserId = u.Id AND r2.Name IN ('SystemAdmin', 'Admin')
                )
                {whereConditions}
            )
            SELECT * FROM FilteredUsers {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        using var connection = new SqlConnection(_connectionString);
        
        var parameters = new
        {
            SearchTerm = request.SearchTerm,
            Status = (int?)request.Status,
            Offset = (request.PageNumber - 1) * request.PageSize,
            PageSize = request.PageSize
        };

        var users = await connection.QueryAsync<UserListItemDto>(mainQuery, parameters);
        var totalCount = await GetTotalCountAsync(connection, request);

        return new PagedResult<UserListItemDto>(users.ToList(), totalCount, request.PageNumber, request.PageSize);
    }
}
```

#### 4. **API Layer**
```csharp
// Example from AdminController - Query endpoint
[HttpGet("users")]
[Authorize(Policy = "RequireUserFullAccessScope")]
public async Task<ActionResult<GetUsersResponseDto>> GetUsers(
    [FromQuery] GetUsersRequestDto request, CancellationToken cancellationToken)
{
    // Validate query parameters
    ValidationHelper.ValidateResult(new GetUsersValidator().Validate(request));

    // Execute query through application service
    var result = await _adminService.GetUsersAsync(request, cancellationToken);

    return Ok(result);
}
```

### Key Principles to Follow

1. **Commands/Writes**: Always go through domain layer for business rule validation
2. **Simple Queries**: Go through domain if business logic is needed, otherwise use DataQueries
3. **Complex Queries**: Bypass domain layer and use direct SQL via DataQueries for performance
4. **Always validate**: Use FluentValidation for input validation at application boundary, using withState and error codes
5. **Register DI**: Don't forget to register new services and interfaces in dependency injection
6. **Follow naming**: Use consistent naming patterns (RequestDto, ResponseDto, Validator, Handler)
7. **Async/Await**: Use async patterns throughout for I/O operations
8. **Error handling**: Let domain exceptions bubble up, handle infrastructure exceptions appropriately
9. **File Organization**: Each class/record should be in a separate file, unless explicitly specified to have multiple classes/records in the same file. For DTOs and validators, follow the naming convention: `{ActionName}{RequestDto|ResponseDto|Validator}` (e.g., `CreateAdminRequestDto`, `CreateAdminResponseDto`, `CreateAdminValidator`)

### Future: Simple Services with Vertical Slice

For simpler microservices, we will adopt a vertical slice architecture pattern where each feature is organized by business capability rather than technical layer. This will be documented when those services are implemented.

**Note**: Current services (User Service) use DDD with layered architecture. Follow the patterns above for consistency.