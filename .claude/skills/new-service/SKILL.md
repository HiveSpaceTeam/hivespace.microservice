# /new-service — Scaffold a New Microservice

Use this skill when the user asks to create a new microservice, scaffold a service, or spin up a new service.

## Step 1 — Gather inputs

Ask the user:
1. **Service name** — e.g. `Billing`, `Search`, `Analytics` (without the `HiveSpace.` prefix; the script adds it)
2. **Service type** — Full Service (4 layers: Domain/Application/Infrastructure/Api) or Lite Service (Core + Api)?

If the user is unsure which type, use this decision guide:
- Full Service: owns business aggregates with lifecycle rules, or participates in a distributed saga
- Lite Service: primarily orchestrates infrastructure (storage, email, queues) or has a narrow technical scope

## Step 2 — Run the scaffold script

From the repo root:

```powershell
# Full Service
.\scripts\new-service.ps1 -ServiceName HiveSpace.[Name]Service -TemplateName ms-full -AddToSolution

# Lite Service
.\scripts\new-service.ps1 -ServiceName HiveSpace.[Name]Service -TemplateName ms-lite -AddToSolution
```

If the template is not installed yet, the script will auto-install it from `./templates/`.

## Step 3 — Post-scaffold mandatory steps

After the script succeeds, complete these in order:

### Both types
- [ ] Open `src/HiveSpace.[Name]Service/HiveSpace.[Name]Service.[Core|Domain]/Exceptions/[Name]DomainErrorCode.cs`
- [ ] Choose a unique error code prefix (e.g. `PAY4xxx` for PaymentService) — see error code table in CLAUDE.md `## Coding Rules`
- [ ] Replace the placeholder prefix `SVC` with the chosen prefix
- [ ] Wire `appsettings.json` connection strings: `DefaultConnection` (SQL Server), `Identity:Authority`, `Identity:Audience`

### Full Service only
- [ ] Verify project reference chain is correct: Domain ← Application ← Infrastructure ← Api (never the reverse)
- [ ] Confirm `Domain` has no reference to `Application`, `Infrastructure`, or `Api`

### Lite Service only
- [ ] Verify `Core` is referenced by `Api` (not the other way around)

## Step 4 — Build verification

```bash
dotnet build src/HiveSpace.[Name]Service
```

Fix any reference or namespace errors before proceeding.

## Step 5 — First migration

```bash
# Full Service
dotnet ef migrations add InitialCreate \
  --project src/HiveSpace.[Name]Service/HiveSpace.[Name]Service.Infrastructure \
  --startup-project src/HiveSpace.[Name]Service/HiveSpace.[Name]Service.Api

# Lite Service
dotnet ef migrations add InitialCreate \
  --project src/HiveSpace.[Name]Service/HiveSpace.[Name]Service.Core \
  --startup-project src/HiveSpace.[Name]Service/HiveSpace.[Name]Service.Api
```

## Step 6 — Update registry

Add the new service to AGENTS.md service table and to the Architecture tree in CLAUDE.md.

## Step 7 — Report to user

Print a summary:
- Service name and type
- Projects created and added to solution
- Error code prefix chosen
- Next steps: implement first domain model / feature, add to API Gateway routing
