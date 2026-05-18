Run this verification checklist after completing a backend story. Check every applicable item and fix failures before marking the story done.

Context:
- Read local `AGENTS.md` and `CLAUDE.md` if present.
- Read the feature `spec.md`, `plan.md`, and `tasks.md` under `../hivespace.spec/specs/[feature-name]/`.

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

Report:
- List files created and modified.
- List verification commands and results.
- If more stories remain, show the next story scope from `tasks.md`.
- Remind the user to run `/wrap-up` in `hivespace.spec` when the full feature is shipped.
