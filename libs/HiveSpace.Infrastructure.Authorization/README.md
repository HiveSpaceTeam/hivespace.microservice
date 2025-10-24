# HiveSpace Infrastructure Authorization

This library provides shared authorization attributes and policies for all HiveSpace microservices.

## 📚 Documentation

- **📖 This README** - Quick start guide and API reference (use this for new services)
- **🔄 [Migration Guide](MIGRATION-GUIDE.md)** - Only for existing services with local authorization code
- **🏗️ [Implementation Summary](IMPLEMENTATION-SUMMARY.md)** - Technical architecture and design decisions

## Features

- **Role-based authorization attributes** that work across all services
- **Reusable authorization policies** for consistent security
- **Service-specific scope validation** while maintaining shared role logic
- **Easy integration** with existing IdentityServer setup

## Installation

Add the package reference to your service project:

```xml
<PackageReference Include="HiveSpace.Infrastructure.Authorization" />
```

## Quick Start

> 💡 **For New Services**: This is all you need! No migration required.  
> 🔄 **For Existing Services**: See [Migration Guide](MIGRATION-GUIDE.md) if you have local authorization code.

### 1. Configure Authorization in Your Service

```csharp
// In Program.cs or ServiceCollectionExtensions.cs
using HiveSpace.Infrastructure.Authorization.Extensions;

// For JWT consumer services (Order, Catalog, Basket, etc.)
services.AddHiveSpaceAuthorization("order.fullaccess");

// For IdentityServer host service (User Service) - LocalApi only
services.AddHiveSpaceAuthorizationForLocalApi("user.fullaccess");

// For hybrid services (if needed) - both JWT Bearer and LocalApi
services.AddHiveSpaceAuthorization("hybrid.fullaccess", useLocalApi: true, useJwtBearer: true);
```

### 2. Use Authorization Attributes

```csharp
using HiveSpace.Infrastructure.Authorization;

[ApiController]
public class OrderController : ControllerBase
{
    [HttpPost]
    [RequireAdmin] // Only admins can create orders manually
    public async Task<IActionResult> CreateOrder() { }

    [HttpGet("my-orders")]
    [RequireUser] // Sellers and customers can view their orders
    public async Task<IActionResult> GetMyOrders() { }

    [HttpDelete("{id}")]
    [RequireSystemAdmin] // Only system admin can delete orders
    public async Task<IActionResult> DeleteOrder(int id) { }
}
```

## Available Authorization Attributes

| Attribute              | Description           | Allowed Roles                        |
| ---------------------- | --------------------- | ------------------------------------ |
| `[RequireSystemAdmin]` | Highest level access  | SystemAdmin                          |
| `[RequireAdmin]`       | Administrative access | Admin, SystemAdmin                   |
| `[RequireSeller]`      | Store management      | Seller                               |
| `[RequireCustomer]`    | Customer only         | Customer (no role)                   |
| `[RequireUser]`        | General user access   | Seller, Customer                     |
| `[RequireAdminOrUser]` | Mixed access          | Admin, SystemAdmin, Seller, Customer |

## Service-Specific Usage

### Order Service Example

```csharp
// Program.cs
services.AddHiveSpaceAuthorization("order.fullaccess");

// Controllers
[RequireAdmin]
public async Task<IActionResult> AdminOrderReport() { }

[RequireSeller]
public async Task<IActionResult> SellerOrderDashboard() { }

[RequireUser]
public async Task<IActionResult> MyOrders() { }
```

### Catalog Service Example

```csharp
// Program.cs
services.AddHiveSpaceAuthorization("catalog.fullaccess");

// Controllers
[RequireSeller]
public async Task<IActionResult> CreateProduct() { }

[RequireAdmin]
public async Task<IActionResult> ModerateProducts() { }

// Public endpoint - no authorization needed
public async Task<IActionResult> GetProducts() { }
```

### Basket Service Example

```csharp
// Program.cs
services.AddHiveSpaceAuthorization("basket.fullaccess");

// Controllers
[RequireUser] // Customers and sellers can manage baskets
public async Task<IActionResult> AddToBasket() { }

[RequireAdmin] // Admins can view all baskets
public async Task<IActionResult> GetAllBaskets() { }
```

## Advanced Usage

### Custom Policy Names

```csharp
[Authorize(Policy = HiveSpaceAuthorizeAttribute.SystemAdmin.Policy)]
public async Task<IActionResult> AdvancedSystemConfig() { }
```

### Service-Specific Base Authentication

```csharp
[Authorize(Policy = HiveSpaceAuthorizeAttribute.Authenticated.OrderService)]
public async Task<IActionResult> BasicOrderEndpoint() { }
```

## Role Hierarchy

```
SystemAdmin (highest)
├── Can access: All policies
├── Use cases: System configuration, global settings

Admin
├── Can access: RequireAdmin, RequireAdminOrUser, base auth
├── Use cases: User management, content moderation

Seller
├── Can access: RequireSeller, RequireUser, RequireAdminOrUser, base auth
├── Use cases: Store management, product management

Customer
├── Can access: RequireCustomer, RequireUser, RequireAdminOrUser, base auth
├── Use cases: Shopping, profile management
```

## Migration from Service-Specific Authorization

### Before (User Service only)

```csharp
using HiveSpace.UserService.Api.Attributes;

[RequireAdmin]
public async Task<IActionResult> CreateUser() { }
```

### After (Shared across all services)

```csharp
using HiveSpace.Infrastructure.Authorization;

[RequireAdmin]
public async Task<IActionResult> CreateUser() { }
```

The API remains exactly the same - just change the using statement!

## Testing Authorization

```csharp
[Test]
public async Task AdminEndpoint_WithCustomerRole_ShouldReturn403()
{
    // Test with different roles
    var client = CreateAuthenticatedClient("Customer");
    var response = await client.GetAsync("/admin/users");

    response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
}
```
