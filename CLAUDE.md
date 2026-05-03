# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Tech Stack

- .NET 8, ASP.NET Core Minimal APIs or Controller
- Entity Framework Core 8
- Mediator for CQRS pattern (source-generated) or using interface with implementation
- FluentValidation for request validation
- Scalar for API documentation (OpenAPI)

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

Before implementing any new feature, first check the existing service architecture. If the target service already has a fixed Application-layer pattern, follow it. Ask which pattern to use only when the target context is genuinely undecided, new, or an intentional exception:

**Option A — CQRS (MediatR)**
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

**Option B — Service / Interface**
A service class groups related operations behind an interface. Use this when multiple operations share state, context, or helpers that would be awkward to repeat across individual handlers.

```text
Application/
  Interfaces/
    I[Feature]Service.cs
  Services/
    [Feature]Service.cs
```

**When to ask:** Ask only when the service architecture or feature context does not already decide the pattern. The two patterns are not equivalent, and agents must not mix them without explicit approval. If the existing service or aggregate already uses one pattern, follow it unless the user explicitly approves an exception.

→ Feature implementation patterns, DDD building blocks, sagas: `.claude/docs/feature-implementation.md`

## Build & Run

```bash
# From repo root — always run in this order
dotnet restore
dotnet build   # Expect 6 nullability warnings in HiveSpace.Domain.Shared; non-blocking

# Run individual services
cd src/HiveSpace.ApiGateway/HiveSpace.YarpApiGateway && dotnet run      # http://localhost:5000, no DB
cd src/HiveSpace.UserService/HiveSpace.UserService.Api && dotnet run    # https://localhost:5001, requires SQL Server
cd src/HiveSpace.CatalogService/HiveSpace.CatalogService.Api && dotnet run  # requires SQL Server + RabbitMQ + Kafka
cd src/HiveSpace.OrderService/HiveSpace.OrderService.Api && dotnet run  # https://localhost:5002, requires SQL Server + RabbitMQ + Kafka

# EF Core migrations (run from project root, targeting the Infrastructure project)
dotnet ef migrations add <Name> --project src/HiveSpace.OrderService/HiveSpace.OrderService.Infrastructure --startup-project src/HiveSpace.OrderService/HiveSpace.OrderService.Api
dotnet ef database update --project src/HiveSpace.OrderService/HiveSpace.OrderService.Infrastructure --startup-project src/HiveSpace.OrderService/HiveSpace.OrderService.Api
```

No test projects exist — `dotnet test` returns immediately.

**Infrastructure requirements**: SQL Server on `localhost:1433` (sa/Passw0rd123!), RabbitMQ on `localhost:5672` (guest/guest), Kafka on `localhost:9092`.

Expected startup warnings: Duende IdentityServer license and MediatR license reminders — development use is permitted.

## Architecture

Clean Architecture / DDD. Each service has four layers: `Domain → Application → Infrastructure → Api`.

```
src/
├── HiveSpace.ApiGateway/HiveSpace.YarpApiGateway/     # YARP reverse proxy, no DB
├── HiveSpace.UserService/                              # Identity & auth (Duende IdentityServer)
├── HiveSpace.CatalogService/                           # Product catalog
├── HiveSpace.OrderService/                             # Orders, cart, coupons, checkout saga
├── HiveSpace.PaymentService/                           # Payment processing
├── HiveSpace.MediaService/                             # Media/file handling (Azure Blob + Functions)
└── HiveSpace.NotificationService/                      # Notifications (email, in-app, SignalR)
libs/
├── HiveSpace.Core/                                     # Exceptions, filters, helpers, pagination models
├── HiveSpace.Domain.Shared/                            # AggregateRoot, Entity, ValueObject, IDomainEvent
├── HiveSpace.Application.Shared/                       # ICommand, IQuery, ICommandHandler, IQueryHandler
├── HiveSpace.Infrastructure.Messaging/                 # MassTransit/Kafka/RabbitMQ abstractions
├── HiveSpace.Infrastructure.Messaging.Shared/          # Cross-service saga contracts (commands/events)
├── HiveSpace.Infrastructure.Authorization/             # HiveSpaceAuthorizeAttribute, policy helpers
└── HiveSpace.Infrastructure.Persistence/               # Idempotence, EF interceptors (audit, soft-delete), transaction service
```

All packages are centrally versioned in `Directory.Packages.props` — never specify versions in `.csproj` files.

→ Startup file conventions (`Program.cs`, `HostingExtensions.cs`, `ServiceCollectionExtensions.cs`): `.claude/docs/startup-conventions.md`

## Service Types — Quick Reference

| Service | Type | Application layer | API surface |
|---------|------|-------------------|-------------|
| UserService | **Full** | Service-based | Controllers |
| CatalogService | **Full** | CQRS | Controllers |
| OrderService | **Full** | CQRS | Minimal Endpoints |
| PaymentService | **Full** | CQRS | Mixed |
| MediaService | **Lite** | CQRS | Minimal Endpoints |
| NotificationService | **Lite** | CQRS | Minimal Endpoints |

The Application layer pattern in the table above is **fixed per service**. Agents must follow it and must not introduce the other pattern without explicit user approval. Specifically: UserService uses Service-based; all other services use CQRS exclusively.

Full detail on layouts, mandatory rules, and new service checklists: `.claude/docs/service-architecture.md`

## Coding Rules

→ Error handling, DI lifetime, validation pipeline, IUserContext, DTOs, integration events, async, monetary values, one-type-per-file: `.claude/docs/coding-rules.md`

## Linked Documentation Files

`CLAUDE.md` and `AGENTS.md` are kept in sync by a PostToolUse hook (`.claude/hooks/sync-docs.sh`). After editing either file, you **must** update the other:
- Changes to service type table in CLAUDE.md → update the service table in AGENTS.md
- Changes to the service table or rules in AGENTS.md → update service type table in CLAUDE.md

## PR Process

Never run `gh pr create` directly. A PreToolUse hook (`.claude/hooks/guard-pr.sh`) blocks it. Required flow:
1. Run `bash scripts/sync-config.sh` to sync all `appsettings.json` / `local.settings.json` to `hivespace.config/`
2. Run `npx gitnexus analyze` to sync the GitNexus index with current changes
3. Tell the user to **start a new session** in this repository
4. In the new session, run `/review` to review all current changes
5. Apply any fixes from the review
6. Only then: `gh pr create`

## Git Commit Guardrails

- Agents must never stage or commit any `*.json` file.
- If a task changes one or more `*.json` files, agents must ask the user to add/stage those JSON files manually.
- Agents can stage and commit only non-JSON files after user confirmation that JSON staging is handled.
- After finishing a task, agents must delete temporary files they created (for example: ad-hoc error logs, scratch/debug files, or one-off investigation artifacts) unless the user explicitly asks to keep them.
- Agents must not stage or commit temporary files created only for debugging or task tracking.

<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **hivespace.microservice** (6611 symbols, 16274 relationships, 300 execution flows). Use the GitNexus MCP tools to understand code, assess impact, and navigate safely.

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
