# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**EIMS** (Education Institute Management System) is a comprehensive school/institute management system built with ASP.NET Core 8.0 MVC, managing student records, academics, employee data, financial transactions, and attendance tracking.

## Tech Stack

- **Framework**: ASP.NET Core 8.0 MVC with Razor Views
- **Language**: C# / .NET 8.0
- **ORM**: Entity Framework Core 7.0 (code-first migrations)
- **Database**: PostgreSQL or MS SQL Server — provider chosen at runtime via the `DatabaseProvider` setting in `SMS_App/appsettings.json` (currently `"SqlServer"`; deployed instances often flip this to `"PostgreSQL"`)
- **Auth**: ASP.NET Core Identity with claim-based authorization policies
- **Background Jobs**: Hangfire (disabled by default; storage matches the active DB provider)
- **Logging**: Serilog
- **Mapping**: AutoMapper 15.1.1
- **PDF Reports**: QuestPDF (Community license) — builder classes in `SMS_App/Utilities/Reports/`
- **External**: GreenWeb SMS API, SMTP email

## Build & Run Commands

Paths below use `/`; on Windows PowerShell swap in `\`.

```bash
# Build entire solution
dotnet build

# Run application (default: https://localhost:5001)
dotnet run --project SMS_App/SMS_App.csproj

# EF Core migrations — run from the SMS.DB directory (DesignTimeDbContextFactory lives there
# and reads DatabaseProvider + connection string from ../SMS_App/appsettings.json)
dotnet ef database update
dotnet ef migrations add <MigrationName> -o Migrations_SqlServer
dotnet ef migrations remove
```

**Tests**: there is no automated test project in the solution (`SchoolManagementSystem.sln` has the 6 layer projects only). The remote `TestProject_ForTestingPurpose` branch is not merged. Verify changes by building and running the app.

The SQL Server migration history lives in `SMS.DB/Migrations_SqlServer/`. The PostgreSQL
database is provisioned from a dump (see `Resources/docker/setup-eims-postgres.ps1`), not from
code-first migrations. `DesignTimeDbContextFactory` picks the provider and decrypts/loads the
connection string the same way `Program.cs` does.

## Architecture — 6-Layer Dependency Chain

```
SMS_App          → Presentation (Controllers, Views, ViewModels)
SMS.BLL          → Business Logic (Manager classes)
SMS.DAL          → Data Access (Repository pattern)
SMS.DB           → EF Core DbContext + Migrations
SMS.Entities     → Domain models, enums, ViewModels (no logic)
SMS.Frameworks   → Shared utilities, tag helpers, encryption
```

**Strict rule**: Dependencies flow downward only. Controllers → Managers → Repositories. Never skip layers; controllers must never call repositories directly. (`ExpensTypesController` is an existing violation — it injects `IExpenseTypeRepository`; don't copy it.)

## Key Architectural Patterns

### Repository Pattern
- All repositories inherit `Repository<T>` (`SMS.DAL/Repositories/Base/Repository.cs`), which implements `IRepository<T>` (`SMS.DAL/Contracts/Base/`)
- Interfaces live in `SMS.DAL/Contracts/`, implementations in `SMS.DAL/Repositories/`
- Standard async CRUD: `GetByIdAsync()`, `GetAllAsync()`, `AddAsync()`, `UpdateAsync()`, `RemoveAsync()`
- The base `Add/Update/Remove` methods each call `SaveChangesAsync()`, and they **catch exceptions and return `false`** (logging only to `Console`). A `false` result can hide the real EF error — reproduce it in a custom repository method or check the console when debugging. `UpdateAsync` marks the whole entity `Modified` rather than relying on change tracking.
- `SMS.BLL/Managers/Base/Manager.cs` is the equivalent base for managers.

### Manager Pattern (Business Logic)
- All business logic in Manager classes under `SMS.BLL/Managers/`
- Interfaces in `SMS.BLL/Contracts/`
- Managers inject repositories; controllers inject managers only
- All manager/repository DI registrations live in `SMS_App/Configurations/DependencyInjectionConfiguration.cs` (`Addservices()`), not in `Program.cs`

### Claim-Based Authorization (Not Role-Based)
- All authorization uses policies defined in `SMS_App/Configurations/AuthorizationPolicies.cs`
- Decorate actions with `[Authorize(Policy = "[EntityName]Policy")]`
- Policy name and claim string are different things: e.g. policy `IndexAcademicClassesPolicy` requires claim `"View Academic Class"`. Policy names contain historical typos (e.g. `CraeteAcademicExamTypePolicy`) that controllers and `SiteMap.Config` reference verbatim — grep before renaming.
- Claims stored in `ClaimStores` entity and assigned via roles
- Global auth filter in `Program.cs` requires authenticated user for all routes
- The sidebar/nav is data-driven from `SMS_App/SiteMap.Config` (loaded as a singleton `SiteMap`). Each `<item>` names a `Controller`, `Action`, and the policy in `Claim=`; a new screen isn't reachable from the menu until it has an entry there.

### ZKTeco Attendance Machines
`Controllers/IClockController.cs` serves the `/iclock/*` ADMS push endpoints that terminal firmware has hard-coded, so those routes can't be renamed or moved into an area. It is `[AllowAnonymous]` and trusts only a registered device serial (`SN`); responses must stay plain text. Direct-socket device access lives in `SMS.BLL/ZKTeco/ZkClient.cs`.

### AutoMapper
- Profiles configured in `SMS_App/Utilities/AutoMapperConfiguration/AutoMapperProfile.cs`
- Injected as `IMapper` into managers and controllers

### Areas
Four areas: `API`, `Identity`, `SMSAPP`, `Student` — routed as `{area:exists}/{controller=Home}/{action=Index}/{id?}`

## Configuration & Secrets

### Database Provider Selection
`Program.cs` reads `DatabaseProvider` (`"PostgreSQL"` or `"SqlServer"`, default `SqlServer`) and configures the matching `DbContext`, Hangfire storage, and migration history schema (`public` vs `dbo`).

### Connection Strings (provider-dependent)
- **SqlServer**: the `DefaultConnection` value is AES-encrypted, decrypted in `Program.cs` via `AesEncryptionHelper.Decrypt()` with keys from env vars `AES_KEY` / `AES_IV` (defaults `"1234567890123456"`). Use the `Resources/deployment/EncryptConnString/` console app to produce an encrypted string.
- **PostgreSQL**: the `DefaultConnection` value is used as-is (plain text, no decryption).
- `appsettings.json` also carries several named per-host profiles (`Mac_Docker_DefaultConnection`, `Local_Desktop_DefaultConnection`, `Noble_DefaultConnection`, etc.); `Program.cs` reads `DefaultConnection` only, so swap the desired profile's value into `DefaultConnection` to switch hosts.

### Key appsettings.json Settings
- `DatabaseProvider` — `"PostgreSQL"` or `"SqlServer"`
- `Hangfire.IsEnabled` — set to `true` to enable background jobs; dashboard at `/hangfire`
- `PhoneSMSSetup` — GreenWeb SMS API token and endpoint
- `Serilog.MinimumLevel` — default `"Error"`

### Data Protection Keys
Persisted to `SMS_App/Keys/` for shared hosting stability. App name: `"SMS_App"`.

### User Secrets ID
`aspnet-SchoolManagementSystem-75AC8697-780A-4215-A7FE-E83F96B3C344`

## Adding a Complete CRUD Feature

1. Define entity in `SMS.Entities/` with data annotations
2. Add `DbSet<T>` to `SMS.DB/ApplicationDbContext.cs`
3. `dotnet ef migrations add Add[EntityName]`
4. Create `I[Entity]Repository` in `SMS.DAL/Contracts/` and implement in `SMS.DAL/Repositories/`
5. Create `I[Entity]Manager` in `SMS.BLL/Contracts/` and implement in `SMS.BLL/Managers/`
6. Create ViewModels in `SMS_App/ViewModels/[Feature]/`
7. Create Controller at `SMS_App/Controllers/[Entity]Controller.cs` — inject manager only
8. Create Views at `SMS_App/Views/[Entity]/`
9. Register manager + repository in `SMS_App/Configurations/DependencyInjectionConfiguration.cs`
10. Add authorization policy in `AuthorizationPolicies.cs`, apply `[Authorize(Policy = "...")]` to actions, and add a menu `<item>` in `SMS_App/SiteMap.Config`

## Code Conventions

- All I/O operations use `async/await`; method names suffixed with `Async`
- `[ValidateAntiForgeryToken]` on all POST/PUT/DELETE actions
- `QueryTrackingBehavior.NoTracking` is set globally where the `DbContext` is registered in `Program.cs`; use `.AsTracking()` only when updating
- Avoid N+1 queries: pre-load all related data in the controller via `Dictionary<K,V>` caches or `.Include()`, never query the DB inside a view loop
- ViewModel naming: `[Entity][Operation]VM` (e.g., `StudentCreateVM`, `StudentListVM`)
- **Cross-platform (Linux deploy)**: format dates for `<input type="date">` with explicit `yyyy-MM-dd` (a culture-default `ToString()` breaks on Linux). PostgreSQL timestamp handling relies on `Npgsql.EnableLegacyTimestampBehavior` (set in `Program.cs`); be mindful of `DateTime` `Kind` when writing new date/time code.

## Deployment & Versioning

- Pushing to **`FinalDeployment`** (the current branch) runs `.github/workflows/deploy.yml`, which builds once and deploys the identical package to **every** client instance (Noble, Mamitonnesa, DMCPS, Unityschoolbd, Curiosity). Treat a push there as a production release; use the workflow's manual "Run workflow" to target a single instance.
- The pipeline strips `appsettings*.json` and never touches `Keys/`, `logs/`, or uploaded images on the servers — per-client config is edited on each server, not in the repo. Adding a client means adding a GitHub Environment plus entries in `deploy.yml` (see `Resources/deployment/CICD.md`).
- The product version is `VersionPrefix` in `Directory.Build.props` (bumped by hand); CI appends build number, commit, and timestamp, and `SMS_App/BuildInfo.cs` reads them back for the UI footer. Local builds show build `0` / commit `local`.

## Notable Files

| File | Purpose |
|------|---------|
| `SMS_App/Program.cs` | Application entry point: DB provider, Identity, auth/session, Hangfire, middleware |
| `SMS_App/Configurations/DependencyInjectionConfiguration.cs` | `Addservices()` — every manager/repository registration |
| `SMS_App/SiteMap.Config` | Menu tree; each item binds a controller/action to a policy |
| `SMS.DB/ApplicationDbContext.cs` | EF Core context, inherits `IdentityDbContext` |
| `SMS_App/Configurations/AuthorizationPolicies.cs` | All claim-based authorization policies |
| `SMS_App/appsettings.json` | App configuration (encrypted connection strings) |
| `SMS_App/GlobalUI.cs` | Global UI constants |
| `SMS.DB/DesignTimeDbContextFactory.cs` | Provider-aware factory used by `dotnet ef` at design time |
| `SMS_App/Utilities/Reports/*.cs` | QuestPDF report builders (admit card, mark sheet, attendance, payments, etc.) |
| `SMS_App/Controllers/ReportsController.cs` | Entry point that invokes the QuestPDF builders |
| `Resources/docker/setup-eims-postgres.ps1` | Spins up a local PostgreSQL 16 container and restores the DB dump |
| `Resources/deployment/` | Linux deploy assets: `deploy.sh`, `eims-instance.service` (systemd), `nginx-school.conf`, `EncryptConnString/` (connection-string encryptor) |
| `Resources/sql*`, `Resources/StudentMigrationScripts/` | Raw SQL — stored procs, views, functions, data-migration scripts run outside EF |
| `SchoolManagementSystem.sln` | Solution file (also a stray `SMS_App/SMS_App.sln` — use the root one) |

## Reference Documentation

- `README.md` — Feature overview and setup instructions  
- `CODING_STANDARDS.md` (root and per-layer) — Naming conventions, SOLID patterns  
- `PERFORMANCE_FIXES.md` — N+1 query optimization example for StudentFeeAllocations  
- `Resources/deployment/CICD.md` — Per-instance GitHub Environments, secrets, and what the pipeline never overwrites
