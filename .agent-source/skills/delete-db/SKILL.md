---
name: delete-db
description: "Safely reset local HiveSpace development SQL Server databases. Use this skill whenever the user asks to delete, drop, clear, wipe, reset, or recreate HiveSpace DBs, including phrases like 'delete db', 'drop all db', 'reset local databases', or 'clear dev SQL'. This skill is intentionally limited to known local HiveSpace app databases and requires exact confirmation before destructive SQL."
---

# Delete Local HiveSpace Databases

Use this skill to drop the known local HiveSpace development databases from SQL Server. The workflow is deliberately conservative because dropping databases is destructive.

## Allowed Scope

Only target these local HiveSpace application databases:

- `CatalogDb`
- `IdentityDb`
- `MediaDb`
- `NotificationDb`
- `OrderDb`
- `PaymentDb`
- `UserDb`

Do not target SQL Server system databases. Do not target custom database names from this skill. If the user asks to drop other names, explain that this skill is limited to the known HiveSpace local development databases.

## Safety Rules

1. Treat every drop request as destructive.
2. Confirm the SQL Server target is local development, normally `127.0.0.1,1433`.
3. Run a read-only discovery query first and report the exact matching databases found.
4. Ask for exact typed confirmation before dropping anything:

   ```text
   yes, drop all listed dbs
   ```

5. Do not run destructive SQL unless the user's latest message contains that exact confirmation phrase.
6. Drop only databases found by the discovery query and only if their names are in the allowed scope.
7. After dropping, run a read-only verification query and report whether zero allowed databases remain.

## Discovery Query

Use this read-only query from `master`:

```powershell
sqlcmd -S 127.0.0.1,1433 -U sa -P "Passw0rd123!" -C -d master -W -s "|" -Q "SELECT name FROM sys.databases WHERE name IN ('CatalogDb','IdentityDb','MediaDb','NotificationDb','OrderDb','PaymentDb','UserDb') ORDER BY name;"
```

If the command fails because of sandboxing or local SQL permissions, request the minimum required command approval and retry. If it fails because SQL Server is not running, report that and stop.

## Confirmation Prompt

After discovery, tell the user the exact databases found and ask:

```text
Reply exactly: yes, drop all listed dbs
```

If no allowed databases are found, report that there is nothing to drop and stop.

## Drop Command

After exact confirmation, build the destructive SQL only for the discovered allowed databases. For each database, use:

```sql
ALTER DATABASE [DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [DatabaseName];
```

For the full default set, the command is:

```powershell
sqlcmd -S 127.0.0.1,1433 -U sa -P "Passw0rd123!" -C -d master -b -Q "ALTER DATABASE CatalogDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE CatalogDb; ALTER DATABASE IdentityDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE IdentityDb; ALTER DATABASE MediaDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE MediaDb; ALTER DATABASE NotificationDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE NotificationDb; ALTER DATABASE OrderDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE OrderDb; ALTER DATABASE PaymentDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE PaymentDb; ALTER DATABASE UserDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE UserDb;"
```

If discovery found only a subset, include only that subset in the destructive SQL.

## Verification Query

Run this read-only query after the drop:

```powershell
sqlcmd -S 127.0.0.1,1433 -U sa -P "Passw0rd123!" -C -d master -W -s "|" -Q "SELECT name FROM sys.databases WHERE name IN ('CatalogDb','IdentityDb','MediaDb','NotificationDb','OrderDb','PaymentDb','UserDb') ORDER BY name;"
```

Report:

- which databases were dropped
- whether the verification returned zero rows
- any database that remained and the error, if available

## Response Shape

Keep the final response short. Example:

```text
Done. Dropped: CatalogDb, IdentityDb, UserDb.

Verification returned 0 matching databases.
```
