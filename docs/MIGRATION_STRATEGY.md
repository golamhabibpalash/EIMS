# Migration strategy: existing schools onto the shared platform

Nothing is migrated automatically. Existing deployments (DMCPS, Unityschoolbd, Curiosity, SchoolBD)
keep running in **single-tenant mode** from the same release until each one is moved on purpose.
No database or deployment is deleted or overwritten by this work.

## Order

1. **New schools first** on the platform (`*.schoolbd.net`).
2. **Pilot 1**: one existing school whose database is reachable from the VPS.
3. **Pilot 2**: a school on a *different* database server/provider, to prove multi-server routing.
4. Remaining schools one at a time, only after both pilots have run cleanly for an agreed period.

## Per-school runbook

| Step | Action | Rollback |
|---|---|---|
| 1. Inventory | Instance, domain, provider + version, DB server, `appsettings` specifics (SMS token, Jwt key, Hangfire), uploaded files size, ZKTeco devices and how they are addressed (IP or host) | — |
| 2. Back up | Full DB backup + copy of `wwwroot/Images`, `wwwroot/Uploads`, `App_Data/AdmissionFiles` | restore |
| 3. Schema check | Compare the school's schema with the current model (the platform runs the same code); apply any missing `Resources/sql/*` scripts. **Do not** run `tenant-init` on an existing DB expecting changes: `EnsureCreated` is a no-op on a non-empty database. | restore backup |
| 4. Dedicated login | Create a DB login used only by the platform for this school (least privilege: the school DB only) | drop login |
| 5. Register | `tenant-add`, `tenant-db` (connectivity is tested), **do not activate yet** | `tenant-status Disabled` |
| 6. Files | Copy uploads into `{StorageRoot}/{code}/wwwroot/Images|Uploads` and `{StorageRoot}/{code}/AdmissionFiles` (same relative paths) | delete copy |
| 7. Test on a staging host | `domain-add --code x --domain x-test.schoolbd.net`, activate, log in, check dashboard, reports, photos, payments, notifications | remove domain |
| 8. Cutover | Freeze writes on the old site (maintenance window), final DB sync if the DB moves, then point the school's DNS to the platform and `domain-add` + `domain-verify` its real domain | point DNS back to the old site (kept running, untouched) |
| 9. Watch | Errors, `/health`, the school's feedback for an agreed period | DNS back |
| 10. Retire | Only then stop the old site. Keep its DB backup and files | — |

## Users and roles

Phase A keeps Identity inside each school database, so existing users, passwords, roles and
permission claims move with the database unchanged. Existing login cookies are not valid on the
platform (they lack the school claim), so users sign in once after cutover. Mobile app users
must point the app at the school's platform domain.

## Compatibility notes

- The platform needs the school DB to be reachable from the VPS (shared-hosting SQL Servers may block
  remote connections or require IP allow-listing).
- Hangfire jobs (attendance SMS etc.) do not run in multi-tenant mode yet. Schools depending on them
  stay single-tenant until Phase B.
- ZKTeco devices addressed by IP cannot be routed to a school yet (Phase B: serial → tenant registry).
