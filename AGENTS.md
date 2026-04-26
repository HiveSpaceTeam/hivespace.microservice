# AGENTS.md — Agent Quick Reference

Full architecture reference: **CLAUDE.md → `## Service Architecture`**

---

## Which service type am I working in?

| Service | Type | Application layer | API surface |
|---------|------|-------------------|-------------|
| UserService | **Full** | Service-based | Controllers |
| CatalogService | **Full** | CQRS | Controllers |
| OrderService | **Full** | CQRS | Minimal Endpoints |
| PaymentService | **Full** | CQRS | Mixed |
| MediaService | **Lite** | CQRS | Minimal Endpoints |
| NotificationService | **Lite** | CQRS | Minimal Endpoints |

---

## Before implementing any feature in a Full Service — ask the user

1. **Application layer**: CQRS (`ICommand`/`IQuery` + handlers) or Service-based (`I[Feature]Service` + `[Feature]Service`)?
2. **API surface**: MVC Controllers or Minimal Endpoints?
3. **Read queries**: EF Core only, or Dapper hybrid for paginated/reporting queries?

For **Lite Services**: CQRS only for all user-facing operations — no `I[Feature]Service` pattern. Handlers always inject repositories, never `DbContext` directly. Always Minimal Endpoints, always EF Core. Cross-cutting infrastructure services (pipeline, router, dedup, rate-limiter) use named interfaces in `Interfaces/` and are not exposed directly by endpoints. Every command/query with user input must have a validator; register all validators + `ValidationPipelineBehavior<,>` in `AddMediatR`.

---

## New service scaffold

```powershell
# Full Service
.\scripts\new-service.ps1 -ServiceName HiveSpace.[Name]Service -TemplateName ms-full -AddToSolution

# Lite Service
.\scripts\new-service.ps1 -ServiceName HiveSpace.[Name]Service -TemplateName ms-lite -AddToSolution
```

After scaffolding:
- Create `[Service]DomainErrorCode.cs` with a unique prefix (Full: `Domain/Exceptions/`; Lite: `Core/Exceptions/`)
- Wire DI in `Extensions/ServiceCollectionExtensions.cs` and `Extensions/HostingExtensions.cs`
- Run first EF migration (see CLAUDE.md checklists for exact commands)

---

## PR process — required before every pull request

**Never run `gh pr create` directly.** When the user asks for a PR:
1. Run `bash scripts/sync-config.sh` to sync all `appsettings.json` / `local.settings.json` to `hivespace.config/`.
2. Run `npx gitnexus analyze` to sync the GitNexus index with current changes.
3. Tell the user: "Please start a new session in this repository."
4. In the new session, run `/review` to review all current changes.
5. Apply any fixes from the review.
6. Only then: `gh pr create`

A `PreToolUse` hook enforces this — `gh pr create` is blocked until the process is followed.

---

## Linked files

CLAUDE.md and AGENTS.md are kept in sync via a `PostToolUse` hook. When you edit either file, you **must** update the other:
- Edit CLAUDE.md `## Service Architecture` → update AGENTS.md service table + hard rules
- Edit AGENTS.md service table or rules → update CLAUDE.md `## Service Architecture`

---

## Hard rules — never violate

- **Never commit `*.json` files** — ask the user to stage JSON manually
- **Full Services only**: always load aggregate → call domain method → save. Never `ExecuteUpdateAsync`
- **Full Services only**: Domain must never reference Application or Infrastructure
- **All CQRS services**: every command/query with user input needs a validator; register validators + `ValidationPipelineBehavior<,>` (from `HiveSpace.Application.Shared.Behaviors`) in `AddMediatR`
- **Never use `System.*` exceptions** — always use `HiveSpace.Domain.Shared.Exceptions` types
- **Never specify package versions in `.csproj`** — all versions live in `Directory.Packages.props`
- **Delete temporary files** (scratch logs, debug artifacts) after the task is done

---

## Error code prefix table

| Service | Prefix | Example |
|---------|--------|---------|
| Shared/common | `APP0xxx` | `CommonErrorCode` in `HiveSpace.Core` |
| UserService | `USR1xxx` | `UserDomainErrorCode` |
| OrderService | `ORD2xxx` | `OrderDomainErrorCode` |
| CatalogService | `CAT3xxx` | `CatalogDomainErrorCode` |
| NotificationService | `NTF4xxx` | `NotificationDomainErrorCode` |
| MediaService | `MED1xxx` | `MediaDomainErrorCode` |
| New services | `[SVC]Nxxx` | Choose next available block |
