# AGENTS.md

Guidance for OpenCode working in **EIMS** (Education Institute Management System) — ASP.NET Core 8.0 MVC, EF Core 7.0.11 running on .NET 8 (don't "fix" the version skew by upgrading EF independently). See `CLAUDE.md` for the full write-up; this file is the compact operating guide.

## Architecture (strict)

6-layer dependency chain, **dependencies flow downward only** — controllers must never call repositories or use the DbContext directly:

```
SMS_App → SMS.BLL (Managers) → SMS.DAL (Repositories) → SMS.DB (DbContext) → SMS.Entities
SMS.Frameworks = shared utilities (no upward deps)
```

- Interfaces in `SMS.*/Contracts/`, implementations in `SMS.*/Managers|Repositories/`.
- All managers/repositories registered by hand in `SMS_App/Configurations/DependencyInjectionConfiguration.cs` (`Addservices()`) — a new CRUD feature must add its `I...Repository/Repository` and `I...Manager/Manager` there.

## Build, run, verify

```bash
dotnet build                                     # root: builds the full solution
dotnet run --project SMS_App/SMS_App.csproj      # https://localhost:5001 (see SMS_App/Properties/launchSettings.json)
dotnet ef database update                        # run from SMS.DB/ (uses its DesignTimeDbContextFactory)
dotnet ef migrations add <Name> -o Migrations_SqlServer   # SqlServer only; run from SMS.DB/
```

- **There is NO test project.** Verify changes by `dotnet build` + running the app.
- AutoMapper profile: `SMS_App/Utilities/AutoMapperConfiguration/AutoMapperProfile.cs` (injected as `IMapper`).
- Hangfire is **disabled by default** (`Hangfire.IsEnabled=false`); dashboard only exists when enabled.

## Database: the biggest gotchas

- `SMS_App/appsettings.json` → `DatabaseProvider` selects `"SqlServer"` or `"PostgreSQL"`. Only `DefaultConnection` is read, and it is **provider-dependent**:
  - **SqlServer**: value is AES-encrypted (AES-128-CBC/PKCS7, base64); decrypted at runtime with env vars `AES_KEY`/`AES_IV` (defaults `"1234567890123456"`). Produce/verify one with `dotnet run -- enc|dec|test "<value>"` from `Resources/deployment/EncryptConnString/`.
  - **PostgreSQL**: value is used as **plain text**.
  - `Program.cs` reads `DefaultConnection` only. `README.md` lists historical per-host profiles (`Noble_…`, `Mac_Docker_…`, etc.) but the current `appsettings.json` contains just `DefaultConnection` — don't expect those keys; to switch hosts, replace the `DefaultConnection` value.
- On startup `Program.cs` runs `CanConnectAsync()` → `EnsureCreatedAsync()` → `DbSeeder.SeedAsync()`. Schema is created from the EF model; **do not assume migrations are applied at runtime**.
- **SqlServer** schema changes use code-first migrations (run from `SMS.DB/`, add with `-o Migrations_SqlServer`). **PostgreSQL is NOT migration-driven** — it is provisioned from a dump (`Resources/docker/setup-eims-postgres.ps1` creates a `postgres:16` container on port 5433 and restores `eimsdb_noble.dump`).
  - Script gotchas: the dump path is hard-coded (`C:\Users\golam\Downloads\eimsdb_noble.dump`) and its port (5433) currently differs from `appsettings.json` (5434) — align them before running.
- CI **never runs migrations** (see `Resources/deployment/CICD.md`); apply EF migrations manually per instance when a release includes schema changes.

## Auth & areas

- Claim-based authorization, **not role-based**: policies live in `SMS_App/Configurations/AuthorizationPolicies.cs`, action policies named e.g. `[Authorize(Policy = "CreateStudentPolicy")]`. A global `AuthorizeFilter` in `Program.cs` requires an authenticated user on every route.
- Permission claims are **not** stored in the login cookie (`AppClaimsPrincipalFactory` keeps it to id/name/roles/stamp; `PermissionClaimsTransformation` rehydrates `AspNetUserClaims`/`AspNetRoleClaims` per request) — ~250 claims would otherwise overflow IIS header limits and cause 400s. Don't bake permissions back into the cookie.
- The claims transformation caches results **15 min per user + security stamp**: claim edits may take up to 15 min to take effect; a security-stamp change (password change, stamp reset) busts the cache immediately.
- Middleware order matters in `Program.cs`: `UseSession()` runs **before** routing/auth; session is 24h with `LoginPath=/Accounts/Login`.
- Policy names contain historical typos (e.g. `CraeteAcademicExamTypePolicy`) referenced verbatim by controllers, `AuthorizationPolicies.cs`, and the ClaimStores seed SQL in `Resources/sql/` — grep before renaming anything.
- A new screen needs a `<item Controller Action Claim>` entry in `SMS_App/SiteMap.Config` or it won't appear in the nav menu. The `Claim=` attribute holds a **policy name** (e.g. `IndexInstitutesPolicy`), not a claim string.
- `Controllers/IClockController.cs` serves `/iclock/*` ADMS push endpoints with hard-coded device-firmware routes: never rename/move them, keep responses plain text, it stays `[AllowAnonymous]` (trusts device `SN` only).
- Areas: `API`, `Identity`, `SMSAPP`, `Student` — routed `{area:exists}/{controller=Home}/{action=Index}/{id?}`.

## Conventions that differ from defaults

- All I/O `async`/`await`, methods suffixed `Async`.
- Controllers inject managers only — never repositories or `DbContext` (`ExpensTypesController` injects `IExpenseTypeRepository`; an existing violation, don't copy it).
- Base `Repository<T>.Add/Update/Remove` each call `SaveChangesAsync()` and **swallow exceptions, returning `false`** (console log only) — a `false` hides the real EF error when debugging.
- `[ValidateAntiForgeryToken]` on every POST/PUT/DELETE action.
- `QueryTrackingBehavior.NoTracking` is the default; use `.AsTracking()` only when updating.
- Avoid N+1: pre-load related data in the controller via `Dictionary<K,V>` caches or `.Include()` — never query the DB inside a view loop (see `PERFORMANCE_FIXES.md`).
- ViewModels named `[Entity][Operation]VM` (e.g. `StudentCreateVM`).
- Format `<input type="date">` values with explicit `yyyy-MM-dd` (culture-default `ToString()` breaks on Linux deploys). `Npgsql.EnableLegacyTimestampBehavior` is enabled in `Program.cs`; mind `DateTime.Kind` with PostgreSQL.

## Misc

- `global.json` pins SDK `8.0.415`; product version built from `Directory.Build.props` (`VersionPrefix`, bumped by hand; build number supplied at publish).
- Use the root `SchoolManagementSystem.sln`, not the stray `SMS_App/SMS_App.sln`.
- Pushing to **`FinalDeployment`** deploys to **all** production instances (see `.github/workflows/deploy.yml`); use the workflow's manual dispatch to target one.
- Data protection keys persist to `SMS_App/Keys/` (don't delete; shared-hosting stability).
- Raw SQL (stored procs, views, data migration) lives in `Resources/sql*` and `Resources/StudentMigrationScripts/` — run outside EF.
- `README.md` is stale in places (says run migrations from `SMS_App/` — actually from `SMS.DB/`; says Serilog uses a SQL sink — `appsettings.json` uses the File sink; lists per-host connection profiles that no longer exist). Trust this file and `CLAUDE.md`.
- **Reports are both RDLC and QuestPDF**: `Controllers/ReportsController.cs` renders `wwwroot/Reports/**/*.rdlc` via `LocalReport` *and* newer reports via QuestPDF builders in `SMS_App/Utilities/Reports/`. Don't remove either path without checking which reports use it.

## References

- `CLAUDE.md` — full architecture/CRUD feature checklist and settings reference
- `CODING_STANDARDS.md` (+ copies in `SMS.BLL/` and `SMS.DB/`) — naming, SOLID, layer responsibilities
- `PERFORMANCE_FIXES.md` — N+1 optimization example for `StudentFeeAllocations`
- `SMS_App/SiteMap.Config` — navigation menu structure