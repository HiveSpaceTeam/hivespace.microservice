Start implementing a backend story from a planned HiveSpace feature.

Step 1 - Load context
- Read local `AGENTS.md` and `CLAUDE.md` if present.
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

Step 3 - Implement according to repo rules
- Follow the local backend architecture: Domain -> Application -> Infrastructure -> Api.
- Use CQRS plus Minimal API for new feature work unless maintaining existing UserService legacy code requires otherwise.
- Keep edits scoped to the selected backend story/task group and any explicitly relevant config, docs/catalog, or verification tasks.
- Add or update integration contracts, endpoints, migrations, consumers, and docs only when required by the selected tasks.
- Verify with the smallest meaningful checks for the changed service, normally `dotnet build` for the affected project and targeted tests if they exist.

Step 4 - Report
- List files changed.
- Summarize verification.
- If more backend tasks remain, identify the next story/task group from `tasks/backend.md`.
- Remind the user to run `/done-story`, then `/wrap-up` in `hivespace.spec` after the full feature ships.
