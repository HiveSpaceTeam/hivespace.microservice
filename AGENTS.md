# AGENTS.md

This file provides guidance to agents working with code in this repository.

## Tech Stack

- .NET 8, ASP.NET Core Minimal APIs for service APIs; IdentityService hosts the auth Razor Pages exception, and UserService remains a temporary controller-based profile/store exception
- Entity Framework Core 8
- Mediator for CQRS pattern (source-generated) for all service feature work; UserService legacy code may still contain service-based implementation
- FluentValidation for request validation
- Scalar for API documentation (OpenAPI)

## Custom Story Commands

| Command | Codex location | Claude Code location | Purpose |
| --- | --- | --- | --- |
| `/start-story` | `.agents/skills/start-story/SKILL.md` | `.claude/commands/start-story.md` | Start a backend story from `../hivespace.spec/specs/[feature-name]` |
| `/done-story` | `.agents/skills/done-story/SKILL.md` | `.claude/commands/done-story.md` | Verify a completed backend story |

Keep paired Codex and Claude command content semantically equivalent.

## Plan First

- Treat any non-trivial task as architectural work.
- Inspect the existing layer before changing it.
- Keep the generated solution minimal, coherent, and production-oriented.

## Behavioral Guardrails

### Think Before Coding

- State assumptions explicitly before implementing when they affect the design.
- If multiple interpretations or tradeoffs exist, surface them instead of picking silently.
- If something is unclear and the repo does not answer it, stop and ask.

### Simplicity First

- Implement only the requested behavior.
- Do not add speculative abstractions, configurability, or impossible-scenario handling.
- If the solution feels larger than the problem, simplify it before proceeding.

### Surgical Changes

- Touch only the code and docs required for the request.
- Do not refactor adjacent code, comments, or formatting unless the task requires it.
- Remove only the unused code or imports created by your own change; mention unrelated cleanup separately.

### Goal-Driven Execution

- Define a concrete verification target before implementing.
- Prefer tests when they exist; otherwise verify with the smallest concrete check that fits the change, such as `dotnet build`, targeted startup, or a focused manual path.
- Every changed line should trace directly to the request and to a verification step.

## Always Clarify Implementation Pattern Before Starting

Before implementing any new feature, first check the existing service architecture. All new feature work uses CQRS plus Minimal API. `UserService` may still contain legacy service-based code, but that legacy pattern must not be extended to new work:

**Option A - CQRS (MediatR)**
Each operation is a discrete command or query handler. Use this when the feature maps cleanly to a single intent (create, update, cancel, list, get).

```text
Application/
  [Feature]/
    Commands/
      [Action][Entity]Command.cs
      [Action][Entity]CommandHandler.cs
      [Action][Entity]Validator.cs
    Queries/
      [Get][Entity]Query.cs
      [Get][Entity]QueryHandler.cs
```

**When to ask:** Ask only when the repo leaves an interface, route, or migration detail unclear. Do not ask to choose between CQRS and service-based patterns for feature work; CQRS is the rule.

See feature implementation patterns, DDD building blocks, and sagas: `docs/agent/feature-implementation.md`

## Build & Run

```bash
# From repo root - always run in this order
dotnet restore
dotnet build   # Expect 6 nullability warnings in HiveSpace.Domain.Shared; non-blocking

# Run individual services
cd src/HiveSpace.ApiGateway/HiveSpace.YarpApiGateway && dotnet run      # http://localhost:5000, no DB
cd src/HiveSpace.UserService/HiveSpace.UserService.Api && dotnet run    # http://localhost:5007, requires SQL Server
cd src/HiveSpace.CatalogService/HiveSpace.CatalogService.Api && dotnet run  # requires SQL Server + RabbitMQ + Kafka
cd src/HiveSpace.OrderService/HiveSpace.OrderService.Api && dotnet run  # https://localhost:5002, requires SQL Server + RabbitMQ + Kafka

# EF Core migrations (run from project root, targeting the Infrastructure project)
dotnet ef migrations add <Name> --project src/HiveSpace.OrderService/HiveSpace.OrderService.Infrastructure --startup-project src/HiveSpace.OrderService/HiveSpace.OrderService.Api
dotnet ef database update --project src/HiveSpace.OrderService/HiveSpace.OrderService.Infrastructure --startup-project src/HiveSpace.OrderService/HiveSpace.OrderService.Api
```

No test projects exist - `dotnet test` returns immediately.

**Infrastructure requirements**: SQL Server on `localhost:1433` (sa/Passw0rd123!), RabbitMQ on `localhost:5672` (guest/guest), Kafka on `localhost:9092`.

Expected startup warnings: Duende IdentityServer license and MediatR license reminders - development use is permitted.

## Architecture

Clean Architecture / DDD. Each service has four layers: `Domain -> Application -> Infrastructure -> Api`.

```text
src/
|-- HiveSpace.ApiGateway/HiveSpace.YarpApiGateway/     # YARP reverse proxy, no DB
|-- HiveSpace.IdentityService/                          # Identity & auth (Duende IdentityServer)
|-- HiveSpace.UserService/                              # User profiles, settings, addresses, stores
|-- HiveSpace.CatalogService/                           # Product catalog
|-- HiveSpace.OrderService/                             # Orders, cart, coupons, checkout saga
|-- HiveSpace.PaymentService/                           # Payment processing
|-- HiveSpace.MediaService/                             # Media/file handling (Azure Blob + Functions)
`-- HiveSpace.NotificationService/                      # Notifications (email, in-app, SignalR)
libs/
|-- HiveSpace.Core/                                     # Exceptions, filters, helpers, pagination models
|-- HiveSpace.Domain.Shared/                            # AggregateRoot, Entity, ValueObject, IDomainEvent
|-- HiveSpace.Application.Shared/                       # ICommand, IQuery, ICommandHandler, IQueryHandler
|-- HiveSpace.Infrastructure.Messaging/                 # MassTransit/Kafka/RabbitMQ abstractions
|-- HiveSpace.Infrastructure.Messaging.Shared/          # Cross-service saga contracts (commands/events)
|-- HiveSpace.Infrastructure.Authorization/             # HiveSpaceAuthorizeAttribute, policy helpers
`-- HiveSpace.Infrastructure.Persistence/               # Idempotence, EF interceptors (audit, soft-delete), transaction service
```

All packages are centrally versioned in `Directory.Packages.props` - never specify versions in `.csproj` files.

See startup file conventions (`Program.cs`, `HostingExtensions.cs`, `ServiceCollectionExtensions.cs`): `docs/agent/startup-conventions.md`

## Service Types - Quick Reference

| Service             | Type     | Application layer | API surface               |
| ------------------- | -------- | ----------------- | ------------------------- |
| IdentityService     | **Lite** | CQRS + ASP.NET Identity | Minimal Endpoints + Razor Pages |
| UserService         | **Full** | Legacy service-based | Legacy controllers |
| CatalogService      | **Full** | CQRS              | Minimal Endpoints         |
| OrderService        | **Full** | CQRS              | Minimal Endpoints         |
| PaymentService      | **Full** | CQRS              | Minimal Endpoints         |
| MediaService        | **Lite** | CQRS              | Minimal Endpoints         |
| NotificationService | **Lite** | CQRS              | Minimal Endpoints         |

The Application layer pattern in the table above is **fixed per service**. Agents must not introduce new service-based feature implementations. Specifically: all new feature work uses CQRS and Minimal API endpoints; `IdentityService` Razor Pages may be maintained for auth UI, and `UserService` legacy service/controller code may be maintained only when required by the existing implementation.

Full detail on layouts, mandatory rules, and new service checklists: `docs/agent/service-architecture.md`

## Coding Rules

**Type name usage**: Use short type names like `Order` by default. Prefer `using` directives, and use fully qualified type names only for a real ambiguity or name collision. If repeated disambiguation is needed in one file, prefer an alias.

See error handling, DI lifetime, validation pipeline, `IUserContext`, DTOs, integration events, async, monetary values, one-type-per-file, and image/media field pattern: `docs/agent/coding-rules.md`

**Value object copying**: Never reconstruct a value object from its own properties (`new Money(x.Amount, x.Currency)`). Use the concrete value object's typed static copy method instead — e.g. `Money.Copy(amount)` or `PhoneNumber.Copy(phone)`. Do not call the base type directly (`ValueObject.Copy(phone)`) in feature code. Both `Copy<T>(T source)` (static) and `Copy<T>()` (instance) are defined on `ValueObject` and produce a shallow clone, but call them through the concrete value object type for readability and intent. Required wherever EF Core OwnsOne tracking demands distinct CLR instances, or to make defensive-copy intent explicit.

**MassTransit consumers — never return silently**: When an entity is not found by its key, `throw new NotFoundException(...)` — never `return` silently. MassTransit retries the message and routes to the dead-letter queue on exhaustion; a silent `return` permanently swallows the failure with no observability. Always use domain exceptions (`NotFoundException`, `InvalidFieldException`) in consumers, never `System.InvalidOperationException`. Publish integration events **before** `SaveChangesAsync()` in consumers — same outbox rule as in handlers. Full rules with examples: `docs/agent/coding-rules.md` § "MassTransit consumers".

## Shared Agent Assets

The repository keeps shared agent assets in canonical root-level locations:

- Shared reference docs live in `docs/agent/`
- Shared skill source lives in `.agent-source/skills/`
- Shared hook logic lives in `scripts/agent/`
- Synced skill copies live in `.agents/skills/` and `.claude/skills/`
- Thin hook wrappers live in `.agents/hooks/` and `.claude/hooks/`
- Codex repo config lives in `.codex/`

When adding or changing a shared skill:

1. Update `.agent-source/skills/`
2. Sync the skill copies into `.agents/skills/` and `.claude/skills/`
3. Update both `AGENTS.md` and `CLAUDE.md` if the workflow or expectations changed

When adding or changing shared hook behavior:

1. Update the implementation in `scripts/agent/`
2. Update the thin wrappers in `.agents/hooks/` and `.claude/hooks/` if needed
3. Update `.codex/hooks.json` or `.claude/settings.json` if the hook wiring changed
4. Update both `AGENTS.md` and `CLAUDE.md` if the workflow or expectations changed

## Linked Documentation Files

`CLAUDE.md` and `AGENTS.md` are kept in sync by a PostToolUse hook wrapper at `.claude/hooks/sync-docs.sh`, which delegates to the shared instruction validator in `scripts/agent/`. After editing either file, you **must** update the other. When shared docs, skills, or hook behavior change, update the mirrored agent folders and both top-level instruction files in the same task.

## PR Process

Required flow:

1. Run `bash scripts/sync-config.sh` to sync all `appsettings.json` / `local.settings.json` to `hivespace.config/`
2. Run `npx gitnexus analyze` to sync the GitNexus index with current changes
3. Run `gh pr create` to open the PR
4. After the PR is open, ask the user to **start a new session** in this repository
5. In the new session, ask the user to run `/review` to review all current changes
6. Apply any fixes from the review and push them to the same branch

## Git Commit Guardrails

- Agents must never stage any `*.json` file.
- If a task changes one or more `*.json` files, agents must ask the user to add/stage those JSON files manually.
- Agents may commit `*.json` files only when they were already staged by the user; do not unstage or restage those files.
- Agents can stage non-JSON files and commit the resulting staged set after confirming any JSON staging was handled by the user.
- After finishing a task, agents must delete temporary files and temporary folders they created (for example: ad-hoc error logs, scratch/debug files, temporary build folders like `.codex-build`, all _.log files, language-service cache files like `_.csproj.lscache`, or one-off investigation artifacts) unless the user explicitly asks to keep them.
- Agents must not stage or commit temporary files created only for debugging or task tracking.

<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **hivespace.microservice** (7101 symbols, 17631 relationships, 300 execution flows). Use the GitNexus MCP tools to understand code, assess impact, and navigate safely.

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
