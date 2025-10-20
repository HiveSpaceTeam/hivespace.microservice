using Microsoft.AspNetCore.Authorization;

namespace HiveSpace.Infrastructure.Authorization;

/// <summary>
/// Custom authorization attribute for HiveSpace role-based access control.
/// Provides strongly-typed access to authorization policies across all HiveSpace microservices.
/// </summary>
public class HiveSpaceAuthorizeAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Requires SystemAdmin role only (highest level access)
    /// </summary>
    public static class SystemAdmin
    {
        public const string Policy = "RequireSystemAdmin";
    }

    /// <summary>
    /// Requires Admin or SystemAdmin roles (administrative access)
    /// </summary>
    public static class Admin
    {
        public const string Policy = "RequireAdmin";
    }

    /// <summary>
    /// Requires Seller role only (store management access)
    /// </summary>
    public static class Seller
    {
        public const string Policy = "RequireSeller";
    }

    /// <summary>
    /// Requires User roles (Seller + Customer - general user access)
    /// </summary>
    public static class User
    {
        public const string Policy = "RequireUser";
    }

    /// <summary>
    /// Requires Customer role only (regular customer access)
    /// </summary>
    public static class Customer
    {
        public const string Policy = "RequireCustomer";
    }

    /// <summary>
    /// Requires Admin or User roles (both administrative and user access)
    /// </summary>
    public static class AdminOrUser
    {
        public const string Policy = "RequireAdminOrUser";
    }

    /// <summary>
    /// Base policy - requires valid scope only (scope varies per service)
    /// </summary>
    public static class Authenticated
    {
        /// <summary>
        /// User service authentication policy
        /// </summary>
        public const string UserService = "RequireUserFullAccessScope";
        
        /// <summary>
        /// Order service authentication policy
        /// </summary>
        public const string OrderService = "RequireOrderFullAccessScope";
        
        /// <summary>
        /// Basket service authentication policy
        /// </summary>
        public const string BasketService = "RequireBasketFullAccessScope";
        
        /// <summary>
        /// Catalog service authentication policy
        /// </summary>
        public const string CatalogService = "RequireCatalogFullAccessScope";
    }

    public HiveSpaceAuthorizeAttribute(string policy) : base(policy) { }
}

/// <summary>
/// Convenience attributes for common authorization scenarios.
/// These can be used across all HiveSpace microservices.
/// </summary>
public class RequireSystemAdminAttribute : AuthorizeAttribute
{
    public RequireSystemAdminAttribute() : base(HiveSpaceAuthorizeAttribute.SystemAdmin.Policy) { }
}

public class RequireAdminAttribute : AuthorizeAttribute
{
    public RequireAdminAttribute() : base(HiveSpaceAuthorizeAttribute.Admin.Policy) { }
}

public class RequireSellerAttribute : AuthorizeAttribute
{
    public RequireSellerAttribute() : base(HiveSpaceAuthorizeAttribute.Seller.Policy) { }
}

public class RequireUserAttribute : AuthorizeAttribute
{
    public RequireUserAttribute() : base(HiveSpaceAuthorizeAttribute.User.Policy) { }
}

public class RequireCustomerAttribute : AuthorizeAttribute
{
    public RequireCustomerAttribute() : base(HiveSpaceAuthorizeAttribute.Customer.Policy) { }
}

public class RequireAdminOrUserAttribute : AuthorizeAttribute
{
    public RequireAdminOrUserAttribute() : base(HiveSpaceAuthorizeAttribute.AdminOrUser.Policy) { }
}