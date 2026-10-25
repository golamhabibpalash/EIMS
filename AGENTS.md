# AGENTS.md

Guidance for OpenCode working in **EIMS** (Education Institute Management System) — ASP.NET Core 8.0 MVC, EF Core 7.0.11 on .NET 8 (don't "fix" the version skew by upgrading EF independently). See `CLAUDE.md` for the full write-up; this is the compact operating guide.

## Architecture (strict)

6-layer dependency chain, **dependencies flow downward only** — controllers must never call repositories or use the DbContext directly:

```
SMS_App → SMS.BLL (Managers) → SMS.DAL (Repositories) → SMS.DB (DbContext) → SMS.Entities
SMS.Frameworks = shared utilities (no upward deps)
```

- Interfaces in `SMS.*/Contracts/`, implementations in `SMS.*/Managers|Repositories/`.
- All managers/repositories are registered by hand in `SMS_App/Configurations/DependencyInjectionConfiguration.cs` (`Addservices()`) — a new CRUD feature must add its `I...Repository/Repository` and `I...Manager/Manager` there.
- `SMS_App/SMS_App.sln` is a stale stray; use the root `SchoolManagementSystem.sln`.

## Build, run, verify

```bash
dotnet build                                     # root: builds the whole solution (6 projects)
dotnet run --project SMS_App/SMS_App.csproj      # https://localhost:5001
dotnet run --project Resources/deployment/EncryptConnString -- enc|dec|test "<value>"
```

- **There is NO test project** and no lint/format script. Verify changes with `dotnet build` and by running the app.
- Startup writes to the configured database: `CanConnectAsync()` → `EnsureCreatedAsync()` → `DbSeeder.SeedAsync()`. It fails fast if the DB is unreachable (not a code bug), and on first run seeds `admin@eims.com` / `Admin@123` as `SuperAdmin`+`Admin`. Check which DB `DefaultConnection` targets before running.
- AutoMapper profile: `SMS_App/Utilities/AutoMapperConfiguration/AutoMapperProfile.cs` (injected as `IMapper`).
- Hangfire is **disabled by default** (`Hangfire.IsEnabled=false`); dashboard exists only when enabled.

## Database & schema (biggest gotchas)

- `SMS_App/appsettings.json` → `DatabaseProvider` selects `"SqlServer"` (current) or `"PostgreSQL"`. Only `DefaultConnection` is read, but the file also contains unused `DefaultConnection_mac`/`DefaultConnection_contoba`; deployed instances keep their own `appsettings*.json` (CI strips them), so edits here never reach production.
  - **SqlServer**: value is AES-encrypted (AES-128-CBC/PKCS7, base64), decrypted with env vars `AES_KEY`/`AES_IV` (defaults `"1234567890123456"`).
  - **PostgreSQL**: value is used as **plain text**.
- Startup only runs `EnsureCreatedAsync()`, which **never adds tables to an existing DB**. The migration history was squashed (`Migrations_SqlServer/…InitialCreate.cs` is empty and the model differ is out of step), so `dotnet ef migrations add` cannot scaffold and `dotnet ef database update` is not a deploy path. Don't "repair" migrations as part of an unrelated task.
- New tables ship as **idempotent per-provider scripts** in `Resources/sql/` (see `UserNotifications_{SqlServer,PostgreSQL}.sql`, `OnlineAdmission_*`), run by hand on each instance **before** deploying. Keep them in sync with `SMS.DB/ApplicationDbContext.cs`; add `ProjectModules`/`ProjectSubModules`/`ClaimStores` rows for new screens (`Seed_Modules_SubModules_ClaimStores_FromSiteMap.sql`, `*_Claims_*.sql`). Code touching a new table should degrade gracefully when the table is absent — CI/deploy never creates schema.
  - Get exact DDL from `Database.GenerateCreateScript()` (per `CLAUDE.md`), not from EF migration SQL. SQL Server scripts must be a single batch with **no `GO`** (DBeaver/shared-hosting consoles reject it).
- PostgreSQL is provisioned from a dump, not migrations (`Resources/docker/setup-eims-postgres.ps1`; hard-coded dump path `C:\Users\golam\Downloads\eimsdb_noble.dump` and port 5433 — edit both to match this machine before running).

## Auth & areas

- Claim-based authorization, **not role-based**: policies live in `SMS_App/Configurations/AuthorizationPolicies.cs`; actions use `[Authorize(Policy = "...")]`. A global `AuthorizeFilter` in `Program.cs` requires an authenticated user on every route, so anything public needs an explicit `[AllowAnonymous]`.
- Permission claims are **not** in the login cookie (`AppClaimsPrincipalFactory` keeps it to id/name/roles/stamp; `PermissionClaimsTransformation` rehydrates permissions from `AspNetUserClaims`/`AspNetRoleClaims`). ~250 claims would overflow IIS headers and cause 400s — don't put them back.
- The claims transformation caches **15 min per user + security stamp**: claim edits can take up to 15 min; a stamp change busts the cache.
- Middleware order matters in `Program.cs`: `UseSession()` runs **before** routing/auth; session is 24h with `LoginPath=/Accounts/Login`.
- Policy names contain historical typos (e.g. `CraeteAcademicExamTypePolicy`) referenced verbatim by controllers, `AuthorizationPolicies.cs`, `SiteMap.Config`, and the claim seed SQL — grep before renaming.
- A screen is invisible until it has a `<item Controller Action Claim>` in `SMS_App/SiteMap.Config`. The `Claim=` attribute holds a **policy name** (e.g. `IndexInstitutesPolicy`), not a claim string.
- `Controllers/IClockController.cs` serves `/iclock/*` ADMS push endpoints hard-coded in device firmware: never rename/move them, keep responses plain text, leave it `[AllowAnonymous]` (trusts only a registered device `SN`).
- Areas: `API`, `Identity`, `SMSAPP`, `Student` — routed `{area:exists}/{controller=Home}/{action=Index}/{id?}`. Online Admission lives in root controllers, not an area.

## Online Admission (guardian portal)

- Public pages: `AdmissionController` (`/Admission`), guardian portal `AdmissionPortalController`, staff `AdmissionSetup` / `AdmissionApplications` / `AdmissionPayments`. Business rules live in `SMS.BLL/Managers/OnlineAdmission/` (`AdmissionRules` is pure logic).
- Guardians are **not** Identity users: `AdmissionGuardianAccount` + its own cookie scheme (`GuardianPortalAuth`, `AdmissionGuardian`) and `GuardianPortalControllerBase`. Portal actions are `[AllowAnonymous]` at the base and enforce the guardian check themselves; mark open actions `[GuardianAnonymous]`.
- Uploads are private under `App_Data/AdmissionFiles/{applicationId}/` (`AdmissionFileStore`) and only streamed by ownership/permission-checked actions. Never move them under `wwwroot`, and never commit runtime uploads (`App_Data` is **not** gitignored).
- Public endpoints are IP rate-limited (`AdmissionRateLimits.Auth` 10/min, `Upload` 30/min, configured in `Program.cs`).
- Every workflow change must go through `IAdmissionRepository.ExecuteInTransactionAsync` (EF retry-on-failure is on, so a bare `BeginTransaction` throws) and write an `AdmissionApplicationHistory` row.

## Real-time notifications (SignalR)

- `UserNotificationManager` (BLL) only persists rows; `SMS_App/Utilities/Notifications/NotificationService.cs` resolves recipients, persists first, then pushes best-effort.
- Hub `SMS_App/Hubs/NotificationHub.cs` is mapped at `/hubs/notifications`, is receive-only, and groups connections by `user:{Context.UserIdentifier}`. Client: `wwwroot/js/notifications.js` + vendored `wwwroot/lib/signalr/`.
- `INotificationService` is registered in `DependencyInjectionConfiguration.cs`, not `Program.cs`. Naming trap: `SMS.Entities/StudentHub.cs` is a domain entity, not SignalR.

## Conventions that differ from defaults

- All I/O `async`/`await`, methods suffixed `Async`.
- Controllers inject managers only — never repositories or `DbContext` (`ExpensTypesController` injects `IExpenseTypeRepository`; an existing violation, don't copy it).
- Base `Repository<T>.Add/Update/Remove` each call `SaveChangesAsync()` and **swallow exceptions, returning `false`** (console log only) — `false` hides the real EF error when debugging.
- `[ValidateAntiForgeryToken]` on every POST/PUT/DELETE action.
- `QueryTrackingBehavior.NoTracking` is the default; use `.AsTracking()` only when updating.
- Avoid N+1: pre-load related data via `Dictionary<K,V>` caches or `.Include()` — never query the DB inside a view loop (see `PERFORMANCE_FIXES.md`).
- ViewModels named `[Entity][Operation]VM` (e.g. `StudentCreateVM`).
- Format `<input type="date">` values with explicit `yyyy-MM-dd` (culture-default `ToString()` breaks on Linux deploys). `Npgsql.EnableLegacyTimestampBehavior` is enabled; mind `DateTime.Kind` with PostgreSQL.

## Logging, deploy, misc

- Unhandled exceptions are written to the `Logs` table by `ExceptionLoggingMiddleware` (view at `/Logs`) and `IAppLogger`; `appsettings.json` `Serilog` sections are **dead config** — there is no Serilog package or wiring.
- Pushing to **`FinalDeployment`** deploys **all five production instances** (`.github/workflows/deploy.yml`); use manual workflow dispatch to target one. The pipeline builds once, strips `appsettings*.json`, and never touches server-side `Keys/`, logs, or uploads.
- Product version is `VersionPrefix` in `Directory.Build.props`, bumped by hand; CI appends build number/commit/timestamp. `global.json` pins SDK `8.0.415`.
- Data protection keys persist to `SMS_App/Keys/` (gitignored; don't delete — shared-hosting stability).
- Raw SQL (stored procs, views, data migration) lives in `Resources/sql*` and `Resources/StudentMigrationScripts/` — run outside EF.
- **Reports are both RDLC and QuestPDF**: `Controllers/ReportsController.cs` uses both `LocalReport` over `wwwroot/Reports/**/*.rdlc` (17 files) **and** QuestPDF builders in `SMS_App/Utilities/Reports/`. Don't remove either path without checking which reports use it.
- `README.md` is stale (wrong migration directory, claims Serilog, lists per-host connection profiles that are mostly gone); trust this file and `CLAUDE.md`.

## References

- `CLAUDE.md` — full architecture, CRUD checklist, schema-script recipe, per-instance deploy notes
- `CODING_STANDARDS.md` (root plus per-layer copies) — naming, SOLID, layer responsibilities
- `PERFORMANCE_FIXES.md` — N+1 optimization example for `StudentFeeAllocations`
- `Resources/deployment/CICD.md` — GitHub Environments, secrets, and what the pipeline never overwrites
