# HiveSpace Authorization Migration Guide

This guide helps other HiveSpace microservices adopt the shared authorization library.

## Overview

The `HiveSpace.Infrastructure.Authorization` library provides:

- **Reusable authorization policies** for all HiveSpace services
- **Custom authorization attributes** for clean, declarative security
- **Consistent role hierarchy** across the entire platform
- **Centralized configuration** with service-specific scopes

## Role Hierarchy

```
SystemAdmin (Highest privilege)
    ↓
Admin (Administrative access)
    ↓
┌─────────────┬─────────────┐
│   Seller    │  Customer   │ (Business users)
└─────────────┴─────────────┘
```

## Integration Steps

### 1. Add Project Reference

In your service's API project (`.csproj`):

```xml
<ItemGroup>
  <ProjectReference Include="..\..\..\libs\HiveSpace.Infrastructure.Authorization\HiveSpace.Infrastructure.Authorization.csproj" />
</ItemGroup>
```

### 2. Update Service Configuration

In your `ServiceCollectionExtensions.cs` or `Program.cs`:

```csharp
using HiveSpace.Infrastructure.Authorization.Extensions;

// Replace your existing authorization setup with:
services.AddHiveSpaceAuthorization("your-service.fullaccess", useLocalApi: true);

// Examples for different services:
// services.AddHiveSpaceAuthorization("order.fullaccess", useLocalApi: true);
// services.AddHiveSpaceAuthorization("basket.fullaccess", useLocalApi: true);
// services.AddHiveSpaceAuthorization("catalog.fullaccess", useLocalApi: true);
```

### 3. Update Controller Imports

Replace old authorization imports with:

```csharp
using HiveSpace.Infrastructure.Authorization;
```

### 4. Apply Authorization Attributes

Replace `[Authorize(Policy = "...")]` with clean attributes:

#### Before (Old approach):

```csharp
[Authorize(Policy = "RequireSystemAdmin")]
public async Task<IActionResult> DeleteEverything() { ... }

[Authorize(Policy = "RequireAdminOrUser")]
public async Task<IActionResult> GetProfile() { ... }
```

#### After (New shared library):

```csharp
[RequireSystemAdmin]
public async Task<IActionResult> DeleteEverything() { ... }

[RequireAdminOrUser]
public async Task<IActionResult> GetProfile() { ... }
```

## Available Authorization Attributes

| Attribute              | Required Roles                                                    | Use Case                                         |
| ---------------------- | ----------------------------------------------------------------- | ------------------------------------------------ |
| `[RequireSystemAdmin]` | SystemAdmin only                                                  | System-wide operations, critical admin functions |
| `[RequireAdmin]`       | Admin + SystemAdmin                                               | Administrative operations                        |
| `[RequireSeller]`      | Seller + Admin + SystemAdmin                                      | Seller-specific operations                       |
| `[RequireCustomer]`    | Customer + Admin + SystemAdmin                                    | Customer-specific operations                     |
| `[RequireUser]`        | All authenticated users                                           | General user operations                          |
| `[RequireAdminOrUser]` | All authenticated users (Admin + SystemAdmin + Seller + Customer) | General authenticated access                     |

## Service-Specific Examples

### Order Service

```csharp
[RequireAdminOrUser]  // Admins can view all orders
public async Task<IActionResult> GetOrders() { ... }

[RequireUser]  // All users can place orders
public async Task<IActionResult> CreateOrder() { ... }

[RequireSystemAdmin]  // Only system admin can delete orders
public async Task<IActionResult> DeleteOrder() { ... }
```

### Catalog Service

```csharp
[RequireSeller]  // Sellers manage their products
public async Task<IActionResult> CreateProduct() { ... }

[RequireAdmin]  // Admins can manage all products
public async Task<IActionResult> ApproveProduct() { ... }

// No attribute needed - public endpoint
public async Task<IActionResult> GetProducts() { ... }
```

### Basket Service

```csharp
[RequireUser]  // All authenticated users can manage baskets
public async Task<IActionResult> AddToBasket() { ... }

[RequireCustomer]  // Only customers can checkout
public async Task<IActionResult> Checkout() { ... }
```

## IdentityServer Configuration

Ensure your service's ApiScope is configured in the User Service's `Config.cs`:

```csharp
public static IEnumerable<ApiScope> ApiScopes =>
new ApiScope[]
{
    // Existing scopes
    new ApiScope("user.fullaccess", "User Full Access"),

    // Add your service scope:
    new ApiScope("order.fullaccess", "Order Service Full Access"),
    new ApiScope("basket.fullaccess", "Basket Service Full Access"),
    new ApiScope("catalog.fullaccess", "Catalog Service Full Access")
};

public static IEnumerable<ApiResource> ApiResources =>
new ApiResource[]
{
    // Existing resources
    new ApiResource("user-api", "User Service API")
    {
        Scopes = { "user.fullaccess" },
        UserClaims = { JwtClaimTypes.Role, JwtClaimTypes.Email, JwtClaimTypes.Name }
    },

    // Add your service resource:
    new ApiResource("order-api", "Order Service API")
    {
        Scopes = { "order.fullaccess" },
        UserClaims = { JwtClaimTypes.Role, JwtClaimTypes.Email, JwtClaimTypes.Name }
    }
};
```

## Testing Authorization

### Valid Test Scenarios

1. **SystemAdmin user**: Can access all endpoints
2. **Admin user**: Can access Admin, Seller, Customer, and User endpoints
3. **Seller user**: Can access Seller and User endpoints (not Admin/SystemAdmin)
4. **Customer user**: Can access Customer and User endpoints (not Admin/SystemAdmin)

### Example Test Cases

```csharp
// Test SystemAdmin access
[Test]
public async Task SystemAdmin_CanAccessAllEndpoints()
{
    var token = GenerateTokenForRole("SystemAdmin");

    // Should succeed for all endpoints
    await AssertEndpointAccessible("/api/admin/users", token);
    await AssertEndpointAccessible("/api/seller/products", token);
    await AssertEndpointAccessible("/api/user/profile", token);
}

// Test Customer restrictions
[Test]
public async Task Customer_CannotAccessAdminEndpoints()
{
    var token = GenerateTokenForRole("Customer");

    // Should fail
    await AssertEndpointForbidden("/api/admin/users", token);

    // Should succeed
    await AssertEndpointAccessible("/api/user/profile", token);
}
```

## Migration Checklist

- [ ] Add project reference to shared authorization library
- [ ] Update service registration to use `AddHiveSpaceAuthorization()`
- [ ] Update controller imports to use shared namespace
- [ ] Replace `[Authorize(Policy = "...")]` with attribute alternatives
- [ ] Remove old local authorization code/classes
- [ ] Update IdentityServer configuration with service scope
- [ ] Test all authorization scenarios
- [ ] Update API documentation with new authorization requirements

## Common Issues

### Issue: "Type or namespace name 'RequireAdmin' could not be found"

**Solution**: Add `using HiveSpace.Infrastructure.Authorization;` import

### Issue: "The scope 'xxx.fullaccess' is not configured"

**Solution**: Add your service scope to User Service `Config.cs` ApiScopes

### Issue: Authorization not working in development

**Solution**: Ensure `useLocalApi: true` is set and JWT Bearer authentication is configured

## Benefits of Migration

✅ **Consistent authorization** across all services  
✅ **Reduced code duplication** - no more local authorization classes  
✅ **Centralized policy management** - single source of truth  
✅ **Type-safe attributes** - compile-time checking  
✅ **Easier maintenance** - update policies in one place  
✅ **Better testing** - standardized authorization behavior

---

For questions or issues, contact the platform team or refer to the User Service implementation as a reference example.
