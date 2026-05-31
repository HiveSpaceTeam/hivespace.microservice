---
name: "start-story"
description: "Start implementing a backend story from a planned HiveSpace feature."
---

Start implementing a backend story from a planned HiveSpace feature.

Step 1 - Load context
- Read local `AGENTS.md` and `CLAUDE.md` if present.
- Read repo implementation rule docs, not only the top-level instructions:
  - `docs/agent/service-architecture.md`
  - `docs/agent/coding-rules.md`
  - `docs/agent/feature-implementation.md` when the affected service is a Full Service
  - Any linked domain-specific rule doc when it directly applies to the story
- Read:
  - `../hivespace.spec/.specify/memory/constitution.md`
  - `../hivespace.spec/shared/event-catalog.md`
  - `../hivespace.spec/shared/api-catalog.md`
- Ask the user which feature and story to implement if not already provided.
- Read `../hivespace.spec/specs/[feature-name]/spec.md`, `plan.md`, and `tasks.md`.
- Read `../hivespace.spec/specs/[feature-name]/tasks/backend.md` for the backend implementation tasks.
- Read relevant `../hivespace.spec/specs/[feature-name]/tasks/config.md`, `tasks/docs-catalog.md`, and `tasks/verification.md` entries when they apply to the selected backend story.
- Read each affected `../hivespace.spec/services/<service-name>/README.md`.

Step 2 - Confirm scope
- State the story, affected service(s), and exact backend task IDs from `tasks/backend.md` for this session, using `tasks.md` as the high-level task index.
- State any relevant config, docs/catalog, and verification task IDs from the detailed task files.
- State what will not be implemented in this session.
- Surface any ambiguity in service ownership, endpoints, events, saga behavior, data ownership, or migration scope before editing.

Step 2.5 - Derive implementation contract
Before editing, state the concrete repo rules that apply to this story:
- Service archetype and project layout: Full or Lite, with exact target project/folder.
- CQRS layout: commands/queries must use per-operation folders (`Commands/[Action][Entity]/`, `Queries/[Get][Entity]/`) with handler and validator colocated.
- DTO layout: keep feature DTOs in the feature-level `Dtos/` folder unless existing local code establishes a narrower documented pattern.
- Validation: every command/query with user-controlled input must have a validator, registered through the MediatR validation pipeline in the application/core layer.
- Domain/application rules: use HiveSpace domain exceptions and service error codes; do not throw `System.*` exceptions from domain/application code.
- Persistence rules: handlers use repositories, not direct `DbContext`, unless the service architecture doc explicitly allows the pattern.
- DI rules: register services/repositories/handlers as scoped unless there is a documented reason not to.
- API rules: use Minimal API endpoints for new feature work, except documented legacy exceptions.
- Messaging rules: if publishing integration events, use service publishers, publish before save, update contracts/catalog/docs when required.
- Verification target: name the smallest meaningful build/test/check before implementation starts.

If any applicable rule is unclear or conflicts with existing code, stop and surface the conflict before editing.

Step 3 - Implement according to the derived contract
- Follow the implementation contract from Step 2.5 exactly.
- Prefer documented repo architecture over abbreviated examples in `AGENTS.md`.
- Keep edits scoped to the selected backend story/task group and any explicitly relevant config, docs/catalog, or verification tasks.
- Add or update contracts, endpoints, migrations, consumers, catalogs, and docs only when required by the selected tasks.
- Before reporting done, verify the contract checklist against the changed files.
- Verify with the smallest meaningful checks for the changed service, normally `dotnet build` for the affected project and targeted tests if they exist.

Step 4 - Report
- List files changed.
- Summarize verification.
- If more backend tasks remain, identify the next story/task group from `tasks/backend.md`.
- Remind the user to run `/done-story`, then `/wrap-up` in `hivespace.spec` after the full feature ships.
