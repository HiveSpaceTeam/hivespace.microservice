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
- Edit CLAUDE.md service type table → update AGENTS.md service table + hard rules
- Edit AGENTS.md service table or rules → update CLAUDE.md service type table

Detailed reference docs (read on demand, not always loaded):
- `.claude/docs/service-architecture.md` — Full/Lite layouts, new service checklists
- `.claude/docs/startup-conventions.md` — File roles, shared helpers, pipeline
- `.claude/docs/feature-implementation.md` — Commands, queries, DDD building blocks, sagas
- `.claude/docs/coding-rules.md` — Error handling, validation, IUserContext, DTOs, events

---

## Startup file conventions

Three files control service startup. Each has a fixed responsibility — never mix them.

| File | Responsibility |
|------|---------------|
| `Program.cs` | Wire `builder` → `app`, call `ConfigureServices` + `ConfigurePipelineAsync`, nothing else |
| `HostingExtensions.cs` | `ConfigureServices()` calls `builder.Services.Add*()` helpers; `ConfigurePipelineAsync()` builds the middleware pipeline |
| `ServiceCollectionExtensions.cs` | Thin `AddApp*()` wrappers — each delegates to a shared lib helper or adds service-specific extras |

### Shared startup helpers — use these, do not re-implement

| Method | Namespace | Purpose |
|--------|-----------|---------|
| `AddHiveSpaceSwaggerGen(title, description)` | `HiveSpace.Core.OpenApi` | SwaggerGen + Bearer security definition |
| `AddHiveSpaceJwtBearerAuthentication(config, scope, configure?)` | `HiveSpace.Infrastructure.Authorization.Extensions` | JWT Bearer + `AddHiveSpaceAuthorization(scope)`; optional callback for service-specific options |
| `AddHiveSpaceControllers()` | `HiveSpace.Core` | `AddControllers` + `CustomExceptionFilter`; returns `IMvcBuilder` for chaining |
| `UseHiveSpaceExceptionHandler()` | `HiveSpace.Core.Extensions` | `UseExceptionHandler` + `ExceptionResponseFactory` pipeline |

**Rule:** `AddApp*()` methods in `ServiceCollectionExtensions.cs` must be thin wrappers — one line delegating to a shared helper, plus any service-specific extras on top. Never duplicate the shared implementation inline.

```csharp
// ✅ Thin wrapper — correct
public static void AddAppOpenApi(this IServiceCollection services)
    => services.AddHiveSpaceSwaggerGen("HiveSpace.CatalogService API", "HiveSpace.CatalogService microservice");

// ❌ Inline re-implementation — wrong
public static void AddAppOpenApi(this IServiceCollection services)
{
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(c => { c.SwaggerDoc(...); c.AddSecurityDefinition(...); ... });
}
```

**UserService exception:** UserService uses LocalApi + Google OAuth — it does NOT use `AddHiveSpaceJwtBearerAuthentication` or `UseHiveSpaceExceptionHandler`.

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

| Service | Prefix(es) | Example |
|---------|------------|---------|
| Shared/common | `APP0xxx` | `CommonErrorCode` in `HiveSpace.Core` |
| UserService | `USR0xxx` | `UserDomainErrorCode` |
| OrderService | `ORD1xxx`, `ORD3–11xxx` | Sub-ranges per aggregate — `OrderDomainErrorCode` |
| CatalogService | `CAT3xxx` | `CatalogDomainErrorCode` |
| PaymentService | `PAY1xxx`, `PAY2xxx`, `PAY3xxx` | Sub-ranges per aggregate — `PaymentDomainErrorCode` |
| NotificationService | `NTF4xxx` | `NotificationDomainErrorCode` |
| MediaService | `MED1xxx` | `MediaDomainErrorCode` |
| New services | `[SVC]Nxxx` | Choose next available block; never renumber existing codes |

<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **hivespace.microservice** (6535 symbols, 16105 relationships, 300 execution flows). Use the GitNexus MCP tools to understand code, assess impact, and navigate safely.

> If any GitNexus tool warns the index is stale, run `npx gitnexus analyze` in terminal first.

## Always Do

- **MUST run impact analysis before editing any symbol.** Before modifying a function, class, or method, run `gitnexus_impact({target: "symbolName", direction: "upstream"})` and report the blast radius (direct callers, affected processes, risk level) to the user.
- **MUST run `gitnexus_detect_changes()` before committing** to verify your changes only affect expected symbols and execution flows.
- **MUST warn the user** if impact analysis returns HIGH or CRITICAL risk before proceeding with edits.
- When exploring unfamiliar code, use `gitnexus_query({query: "concept"})` to find execution flows instead of grepping. It returns process-grouped results ranked by relevance.
- When you need full context on a specific symbol — callers, callees, which execution flows it participates in — use `gitnexus_context({name: "symbolName"})`.

## When Debugging

1. `gitnexus_query({query: "<error or symptom>"})` — find execution flows related to the issue
2. `gitnexus_context({name: "<suspect function>"})` — see all callers, callees, and process participation
3. `READ gitnexus://repo/hivespace.microservice/process/{processName}` — trace the full execution flow step by step
4. For regressions: `gitnexus_detect_changes({scope: "compare", base_ref: "main"})` — see what your branch changed

## When Refactoring

- **Renaming**: MUST use `gitnexus_rename({symbol_name: "old", new_name: "new", dry_run: true})` first. Review the preview — graph edits are safe, text_search edits need manual review. Then run with `dry_run: false`.
- **Extracting/Splitting**: MUST run `gitnexus_context({name: "target"})` to see all incoming/outgoing refs, then `gitnexus_impact({target: "target", direction: "upstream"})` to find all external callers before moving code.
- After any refactor: run `gitnexus_detect_changes({scope: "all"})` to verify only expected files changed.

## Never Do

- NEVER edit a function, class, or method without first running `gitnexus_impact` on it.
- NEVER ignore HIGH or CRITICAL risk warnings from impact analysis.
- NEVER rename symbols with find-and-replace — use `gitnexus_rename` which understands the call graph.
- NEVER commit changes without running `gitnexus_detect_changes()` to check affected scope.

## Tools Quick Reference

| Tool | When to use | Command |
|------|-------------|---------|
| `query` | Find code by concept | `gitnexus_query({query: "auth validation"})` |
| `context` | 360-degree view of one symbol | `gitnexus_context({name: "validateUser"})` |
| `impact` | Blast radius before editing | `gitnexus_impact({target: "X", direction: "upstream"})` |
| `detect_changes` | Pre-commit scope check | `gitnexus_detect_changes({scope: "staged"})` |
| `rename` | Safe multi-file rename | `gitnexus_rename({symbol_name: "old", new_name: "new", dry_run: true})` |
| `cypher` | Custom graph queries | `gitnexus_cypher({query: "MATCH ..."})` |

## Impact Risk Levels

| Depth | Meaning | Action |
|-------|---------|--------|
| d=1 | WILL BREAK — direct callers/importers | MUST update these |
| d=2 | LIKELY AFFECTED — indirect deps | Should test |
| d=3 | MAY NEED TESTING — transitive | Test if critical path |

## Resources

| Resource | Use for |
|----------|---------|
| `gitnexus://repo/hivespace.microservice/context` | Codebase overview, check index freshness |
| `gitnexus://repo/hivespace.microservice/clusters` | All functional areas |
| `gitnexus://repo/hivespace.microservice/processes` | All execution flows |
| `gitnexus://repo/hivespace.microservice/process/{name}` | Step-by-step execution trace |

## Self-Check Before Finishing

Before completing any code modification task, verify:
1. `gitnexus_impact` was run for all modified symbols
2. No HIGH/CRITICAL risk warnings were ignored
3. `gitnexus_detect_changes()` confirms changes match expected scope
4. All d=1 (WILL BREAK) dependents were updated

## Keeping the Index Fresh

After committing code changes, the GitNexus index becomes stale. Re-run analyze to update it:

```bash
npx gitnexus analyze
```

If the index previously included embeddings, preserve them by adding `--embeddings`:

```bash
npx gitnexus analyze --embeddings
```

To check whether embeddings exist, inspect `.gitnexus/meta.json` — the `stats.embeddings` field shows the count (0 means no embeddings). **Running analyze without `--embeddings` will delete any previously generated embeddings.**

> Claude Code users: A PostToolUse hook handles this automatically after `git commit` and `git merge`.

## CLI

| Task | Read this skill file |
|------|---------------------|
| Understand architecture / "How does X work?" | `.claude/skills/gitnexus/gitnexus-exploring/SKILL.md` |
| Blast radius / "What breaks if I change X?" | `.claude/skills/gitnexus/gitnexus-impact-analysis/SKILL.md` |
| Trace bugs / "Why is X failing?" | `.claude/skills/gitnexus/gitnexus-debugging/SKILL.md` |
| Rename / extract / split / refactor | `.claude/skills/gitnexus/gitnexus-refactoring/SKILL.md` |
| Tools, resources, schema reference | `.claude/skills/gitnexus/gitnexus-guide/SKILL.md` |
| Index, status, clean, wiki CLI commands | `.claude/skills/gitnexus/gitnexus-cli/SKILL.md` |

<!-- gitnexus:end -->
