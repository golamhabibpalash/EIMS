# Current architecture (audit, before multi-tenancy)

Audit of the EIMS / SchoolBD code base on branch `Multi-Tenant` (from `FinalDeployment` at
`c9d14949`, version 1.3.0). It describes the system as every school runs it today.

## Stack

| Area | What is used |
|---|---|
| Runtime | ASP.NET Core 8 MVC + Razor views, SDK pinned 8.0.415 (`global.json`) |
| ORM | EF Core **7.0.11** on .NET 8 (deliberate skew) |
| Databases | SQL Server (all 4 production instances) or PostgreSQL, chosen by `DatabaseProvider` |
| Auth | ASP.NET Core Identity (cookie) + JWT bearer for the Flutter app (`Areas/API`) + a separate guardian cookie scheme (online admission) |
| Authorization | ~250 claim-based policies (`AuthorizationPolicies.cs`); permissions loaded per request by `PermissionClaimsTransformation` (memory cache), kept out of cookie/JWT to avoid IIS header overflow |
| Real time | SignalR `NotificationHub`, groups `user:{id}` |
| Jobs | Hangfire (off by default), storage in the school DB |
| Reports | QuestPDF + RDLC (`ReportViewerCore`) |
| Hosting | 3 Windows/IIS shared-hosting sites + 1 Linux VPS, one deployment and one DB per school |

## Layers

`SMS_App` (controllers, views) → `SMS.BLL` (managers) → `SMS.DAL` (repositories) → `SMS.DB`
(`ApplicationDbContext`, 94 DbSets incl. Identity) → `SMS.Entities`. `SMS.Frameworks` holds tag
helpers. 70 controllers, 60 repositories. The base `Repository<T>` catches exceptions and returns
`false`. There was no test project.

## Data and schema

- One `ApplicationDbContext : IdentityDbContext`: school data and Identity users/roles/claims share
  the school's database. `ApplicationUser.UserType/ReferenceId` points into `Employee`/`Student`;
  `Attendance` has a foreign key to the user.
- Startup ran `CanConnect → EnsureCreated → DbSeeder` against the single database before `app.Run()`.
  A slow or unreachable database therefore blocked startup (HTTP 500.37 on IIS).
- Migration history is squashed; `migrations add` cannot scaffold. Schema changes ship as hand-run
  per-provider scripts in `Resources/sql/` (87 files).

## Single-school assumptions found

| Where | Assumption |
|---|---|
| `Program.cs` | One connection string, decrypted at startup; one `AddDbContext` |
| `Program.cs` startup | Blocking DB init + seeding of `admin@eims.com / Admin@123` |
| `GlobalUI` | Static `PageTitle`, `InstituteName`, `Logo`… set in 205 places, shared by all requests |
| Layouts | Branding = "first row of the `Institute` table" |
| `PermissionClaimsTransformation` | Cache key `permclaims:{userId}:{stamp}` |
| `NotificationHub` | Group `user:{userId}` |
| `SmsSendGate` | Process-wide de-dup key `{type}|{number}|{day}` |
| `AdmissionFileStore` | Singleton, root `App_Data/AdmissionFiles` |
| Controllers (11) | Uploads written to `wwwroot/Images/...`, `wwwroot/Uploads/...` and served publicly as static files |
| Hangfire | Storage + job arguments without any school context |
| `IClockController` | ZKTeco devices push to `/iclock/*` identified only by serial (`SN`), often by IP |
| `appsettings.json` | Serilog MSSqlServer sink hard-coded to one school's DB (with a plaintext password in git) |
| Data Protection / JWT | One key ring and one signing key, so a ticket from one site would decrypt on another if served by the same process |

## Weaknesses noted (independent of multi-tenancy)

- Plaintext DB password and SMS token committed in `appsettings.json` (rotate).
- `GlobalUI` statics race between concurrent requests even with one school.
- Static uploads (student photos, NID scans) are reachable without login by URL.
- Repository base hides EF errors behind `false`.
