# Multi-tenant architecture (Phase A, implemented)

One deployment of SchoolBD serves many schools. **One school = one tenant = one database.**
A request is bound to its school from the **Host header only**, through a registry of domains.

```
abc.schoolbd.net ─┐                         ┌─ SchoolBD_PlatformDB (registry)
xyz.schoolbd.net ─┼─ nginx ─ SchoolBD app ──┤
abcschool.edu.bd ─┘   (Host)  (one process) ├─ DB-SERVER-01 (SQL Server) ── abcDB
                                            └─ DB-SERVER-02 (PostgreSQL) ── xyzDB
```

Multi-tenancy is **opt-in** (`MultiTenancy:Enabled`). With it off, the app runs exactly as before on
`DefaultConnection`, so the existing per-school deployments keep working from the same code.

## Platform database (`SchoolBD_PlatformDB`)

`SMS.DB/Platform/PlatformDbContext.cs`, SQL Server or PostgreSQL (`MultiTenancy:Platform:Provider`).
Schema: `Resources/sql/Platform_{SqlServer,PostgreSQL}.sql` or `SMS_App platform init`.

| Table | Purpose / rules |
|---|---|
| `Tenants` | `TenantCode` (unique lowercase slug, immutable), name, `Status` (Provisioning/Active/Suspended/Disabled), soft delete |
| `TenantDomains` | `Domain` **unique** (one domain → one tenant), normalised; `DomainType` (platform subdomain / custom); check constraint `IsActive ⇒ IsVerified`; verification token |
| `DatabaseServers` | provider, host, port, non-secret options; many servers supported |
| `TenantDatabaseConnections` | server + database name + `SecretReference`; filtered unique index = one primary per tenant; status (Pending/Active/Failed/Retired) |
| `PlatformSecrets` | database credentials, **AES-256-GCM ciphertext**; the key (`SCHOOLBD_SECRETS_KEY`) is not in any database |
| `PlatformAuditLogs` | every registry change (who, what, when) |

Not built yet (planned): `TenantSettings`, subscriptions, central identity tables (Phase C).

## Request flow

1. `/build-info` and `/health` are answered first (no school, no DB).
2. `TenantResolutionMiddleware` (`SMS_App/MultiTenancy/`) normalises the host (lowercase, no port or
   trailing dot, IDN→punycode, rejects IPs/single labels/odd characters) and asks
   `ITenantCatalog.ResolveHostAsync`.
3. `TenantCatalog` (`SMS.BLL/MultiTenancy/`) looks the domain up in the registry (cached 60 s,
   unknown hosts 15 s, `Invalidate()` after changes), checks domain active+verified, tenant Active,
   primary DB Active, decrypts the credentials and builds the connection string with the provider's
   `ConnectionStringBuilder` (no keyword injection).
4. Unknown/unverified host → 404, suspended → 403, provisioning/DB not ready → 503. Nothing else runs.
5. The tenant is bound to the request (`HttpContext.Items` + scoped `ITenantContext`); a scope can
   never be re-bound to a different tenant. Log scope gets `TenantId`, `TenantCode`, `DatabaseServerId`.
6. `ApplicationDbContext` is registered **scoped** with an options callback that reads the current
   tenant, so every repository/manager transparently uses that school's database. No DbContext
   pooling. Without a tenant it throws `TenantNotResolvedException` instead of guessing.

Never used to pick a tenant: query strings, `X-Tenant-*` headers, cookies, `X-Forwarded-Host`
(ForwardedHeaders only processes For/Proto from loopback; nginx sends the original `Host`).

## Isolation beyond the database

| Concern | Mechanism |
|---|---|
| Staff/student cookie, guardian cookie, JWT access + refresh | Each carries claim `schoolbd_tenant`; validated on every request (`TenantClaims`). A login from abc is rejected on xyz (cookie → sign-in page, JWT → 401). Old single-tenant cookies without the claim are still accepted in single-tenant mode. |
| Permission cache | key `t:{code}:permclaims:{user}:{stamp}` |
| SignalR | group `t:{code}:user:{id}`, built server-side only; the hub has no client-callable methods |
| SMS duplicate gate | scoped by tenant key |
| Page title / branding (`GlobalUI`) | now per request (`HttpContext.Items`); branding still comes from the school's own `Institute` row, which is now the right database |
| Uploaded files | `ITenantFileStorage`: `{StorageRoot}/{code}/wwwroot/Images|Uploads/...` (same relative paths and URLs) and `{StorageRoot}/{code}/AdmissionFiles`. `/Images/**` and `/Uploads/**` are served from the current school's folder first; shared placeholders fall back to `wwwroot`. Path traversal is rejected by `PhysicalFileProvider`. |
| Startup | No school DB is touched at startup in multi-tenant mode; one school's outage affects only that school's requests. |
| Hangfire | Forced off in multi-tenant mode (jobs carry no tenant yet). |

## Onboarding (platform administrators, on the server)

`dotnet SMS_App.dll platform <command>`. Idempotent and audited; passwords come from env vars.

```
platform init
platform server-add   --name DB-SERVER-01 --provider SqlServer --host 10.0.0.5 --options "TrustServerCertificate=True"
platform tenant-add   --code abc --name "ABC School"
platform tenant-db    --code abc --server DB-SERVER-01 --database abcDB --user abc_app --password-env ABC_DB_PW
platform tenant-init  --code abc --admin-email admin@abc.edu.bd --admin-password-env ABC_ADMIN_PW
platform domain-add   --code abc --domain abc.schoolbd.net --primary     # active at once
platform domain-add   --code abc --domain abcschool.edu.bd               # inactive + token
platform domain-verify --domain abcschool.edu.bd                         # after DNS points here
platform tenant-status --code abc --status Active                        # checks DB first
platform list | tenant-check --code abc
```

`tenant-db` tests connectivity before marking the database Active; `tenant-status Active` refuses
an unreachable database. Custom-domain verification fetches
`https://{domain}/.well-known/schoolbd-domain-verification`, which the app answers with the token
only for a registered, unverified domain.

## Known limits (see roadmap)

Identity is still per school database (a person in two schools has two accounts). No admin UI
yet. ZKTeco push endpoints cannot resolve a school from a bare IP. Hangfire off. Static uploads
are isolated per school but still not behind a login.
