# Security assessment: multi-tenant mode

## Threats and controls (Phase A)

| Threat | Control | Evidence |
|---|---|---|
| Pick another school via `?tenant=`, `X-Tenant-Id`, cookies | Tenant comes only from the Host header + registry; nothing else is read | `TenantResolutionMiddlewareTests.Tenant_headers_query_strings_and_forwarded_host_are_ignored`; manual curl run (abc stays abc) |
| Host-header / `X-Forwarded-Host` spoofing | `XForwardedHost` never enabled; forwarded headers only from loopback; nginx sends `$host`, clears X-Forwarded-Host, default server drops unknown hosts | middleware test; nginx template |
| Unknown or look-alike domain | Strict normalisation (rejects IPs, `@`, `/`, ports with text, bad labels); registry match only; 404 | `HostNormalizationTests`, `Unknown_hosts_never_reach_the_app` |
| Claiming someone's custom domain | Unique domain index; custom domains inactive until verified; check constraint `IsActive ⇒ IsVerified` | `Unverified_custom_domain_does_not_resolve`; SQL constraint |
| Reusing a login from school A on school B | `schoolbd_tenant` claim on every cookie and JWT (access + refresh), checked on each request; mismatch signs out / 401 | `TenantClaimTests`; manual: abc cookie on xyz → login page, abc JWT on xyz → 401, abc refresh on xyz → 401 |
| Same credentials on another school | Each school has its own Identity tables; abc's admin cannot log in on xyz | manual run |
| Request reaching two databases | Scoped DbContext built from the bound tenant; a scope cannot be re-bound; no pooling | `Concurrent_scopes_for_different_schools_get_their_own_database`, `A_scope_cannot_be_switched_to_another_tenant` |
| Cache / SignalR / SMS-gate crossover | Tenant key in every key and group name | `Cache_keys_and_signalr_groups_differ_per_school_for_the_same_user`, `Duplicate_sms_gate_is_per_school` |
| Reading another school's files | Per-school storage folder; static serving from the bound school's folder only; traversal rejected | manual: abc photo via xyz → 404, `..` → 404 |
| Suspended school still used | 403 before MVC, also for the API | `Suspended_school_is_refused_with_403`; manual run |
| DB passwords at rest | AES-256-GCM in `PlatformSecrets`, key only in env; never in APIs, logs or `ToString()` | `SecretAndConnectionStringTests`, `Connection_string_is_never_in_ToString`; DB inspected: ciphertext only |
| Connection-string injection through credentials | Provider `ConnectionStringBuilder` | `Credentials_cannot_inject_connection_keywords` |
| One school DB outage | No school DB at startup; resolution per request; other schools unaffected | manual: xyz suspended/unreachable while abc served |

## Residual risks (open)

1. **Shared secrets**: one Data Protection key ring and one JWT key for all schools. Mitigated by
   the tenant claim; per-tenant key purposes would add defence in depth.
2. **Uploads are public by URL** within a school (student photos, NID scans). Phase B: serve through an
   authorised controller.
3. **Master key loss/leak**: `SCHOOLBD_SECRETS_KEY` decrypts every school's DB password. Keep it out
   of git, back it up offline, restrict the env file to root (chmod 600). Rotation (re-encrypt with
   a new `KeyVersion`) is not automated yet.
4. **Platform admin = server shell**: the `platform` CLI has no separate auth; anyone with shell
   access to the box as the app user can change the registry. Audit log records the OS user.
5. **Custom-domain verification proves DNS routing, not WHOIS ownership.** First registration wins.
   A DNS TXT check is planned.
6. **Plaintext secrets already in git** (`appsettings.json`: a DB password in the Serilog sink, the SMS
   token): rotate them; they are unrelated to this change but now shared by every school on a platform.
7. **ZKTeco `/iclock`** is anonymous and identified by serial. In multi-tenant mode it only works on a
   school's domain; a device pointed at a bare IP gets 404.
8. **Hangfire dashboard/jobs** disabled in multi-tenant mode; re-enabling needs tenant-carrying jobs.

## Operational requirements

- Database servers reachable only from the app host (firewall / private network); never public.
- Dedicated DB login per school with rights on that school's database only.
- `/health` is public but returns only `ok` or `platform database unreachable`.
