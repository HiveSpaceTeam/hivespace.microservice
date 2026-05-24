# IdentityService Development Migrations

Feature `0002-split-identity-service` allows a breaking development reset while
identity-owned data is moved out of UserService.

The target migration path must leave IdentityService owning:

- ASP.NET Identity users, password hashes, roles, claims, and external login links.
- Account status used for sign-in and authorization.
- Failed-login lockout fields.
- Email verification state.
- IdentityServer clients, persisted grants, and operational token state.

UserService keeps profiles, settings, addresses, and stores keyed by the same
public user ID. Do not add profile status, roles, lockout, password, or email
verification state back to UserService migrations.
