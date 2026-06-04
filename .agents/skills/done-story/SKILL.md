---
name: "done-story"
description: "Verify a completed backend story."
compatibility: "HiveSpace backend repo custom command"
---

Run this verification checklist after completing a backend story. Check every applicable item and fix failures before marking the story done.

Context:
- Read local `AGENTS.md` and `CLAUDE.md` if present.
- Read the feature `spec.md`, `plan.md`, `tasks.md`, and `tasks/backend.md` under `../hivespace.spec/specs/[feature-name]/`.
- Read relevant `tasks/config.md`, `tasks/docs-catalog.md`, and `tasks/verification.md` entries when they apply to the completed backend story.

Backend checks:
- New feature work follows CQRS plus Minimal API, except required UserService legacy maintenance.
- Domain entities use private setters, protected parameterless constructors for EF Core, and static factory methods.
- Domain events are added through `AddDomainEvent(...)`.
- Entity removal follows existing soft-delete/status patterns; no accidental hard-delete behavior.
- Monetary values follow the backend money convention and smallest-unit storage where applicable.
- Package references do not add `Version=` attributes in `.csproj` files.
- EF Core table and column naming follows existing service conventions.
- New consumers are idempotent and do not silently swallow missing required data.
- Outgoing integration messages use the transactional outbox pattern.
- New public endpoints are reflected in `../hivespace.spec/shared/api-catalog.md`.
- New integration events, commands, saga messages, failures, and timeouts are reflected in `../hivespace.spec/shared/event-catalog.md`.

Verification:
- Run the smallest meaningful build/test command for the changed service.
- Confirm any generated migration compiles if a migration was added.
- Confirm no unrelated files were changed.
- Treat modified `*.json`, `.env`, user-secrets, or other local runtime config as user-owned unless the story explicitly requires validating committed config shape. Do not replace secrets with placeholders, revert local values, or fail the done check solely because user-managed config contains local secret values; instead list those files separately and remind the user that agents must not stage JSON/env secret files.

Report:
- List files created and modified.
- List verification commands and results.
- List modified JSON/env secret-bearing files separately as "user-staged/user-owned config" when present, without treating them as an agent-staged blocker.
- If more stories remain, show the next backend story/task group from `tasks/backend.md`, using `tasks.md` for dependency order.
- Remind the user to run `/wrap-up` in `hivespace.spec` when the full feature is shipped.
