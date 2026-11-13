# Implementation roadmap

Decisions (2026-11-13): SQL Server **and** PostgreSQL per school, no MariaDB; platform domain
`schoolbd.net` (Cloudflare DNS); Linux VPS; new schools first.

## Phase A: core multi-tenancy (done on branch `Multi-Tenant`, version 1.4.0)

- [x] Repository audit (`CURRENT_ARCHITECTURE.md`)
- [x] Platform DB: tenants, domains, DB servers, DB connections, encrypted secrets, audit; scripts for both providers
- [x] Host-based tenant resolution with registry, normalisation, caching, custom domains + verification token
- [x] Per-request school DbContext across servers/providers; single-tenant fallback unchanged
- [x] No school DB at startup in multi-tenant mode
- [x] Tenant-bound cookies, guardian cookie, JWT access/refresh
- [x] Tenant-scoped permission cache, SignalR groups, SMS gate, per-request `GlobalUI`
- [x] Per-school file storage (uploads, admission files)
- [x] `platform` CLI for onboarding (idempotent, audited)
- [x] `/health` endpoint; nginx + env templates for `*.schoolbd.net`
- [x] `SMS.Tests` project: 59 tests (resolution, isolation, crypto, concurrency)
- [x] Local end-to-end: abc on SQL Server, xyz on PostgreSQL, one process, 400 concurrent mixed requests, no leakage

Not verified yet: custom-domain verification over real HTTPS (local dev certificate could not cover
the test domain), production nginx/certbot setup, real upload through each upload screen in
multi-tenant mode.

## Phase B: operations

1. Platform admin UI (separate platform-admin auth; never for school admins) on top of `ITenantAdministrationManager`
2. Schema baselining per existing school DB + a migration runner (`TenantMigrationService`) and `TenantMigrationHistory`; record `SchemaVersion`
3. Hangfire per tenant: jobs carry `TenantCode`, a job filter binds `ITenantContext` before execution, skip inactive tenants
4. ZKTeco: registry `DeviceSerial → Tenant` in the platform DB for `/iclock/*` when the host is not a school domain
5. Uploads behind authorisation (photos, NID, tickets) instead of static URLs
6. DNS TXT verification for custom domains; automatic certificates for custom domains (Cloudflare for SaaS or on-demand ACME)
7. `TenantSettings` (time zone, locale, feature flags), subscriptions
8. Secrets key rotation (`KeyVersion`), per-tenant Data Protection purposes
9. Pilot migration of one existing school, then a second on another DB server (`MIGRATION_STRATEGY.md`)
10. Docker image + compose for the platform (the current VPS runs systemd services; keep them undisturbed)

## Phase C: separate decisions

- Central identity (one user, memberships and roles per school, tenant switching), which requires
  merging users across school databases and re-homing `ApplicationUser` references.
- .NET 10 / EF Core 10 upgrade (EF 7 → 10 breaking changes, Npgsql timestamp behaviour).
