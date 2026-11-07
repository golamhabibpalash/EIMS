# CI/CD — one branch, one commit, every instance

`.github/workflows/deploy.yml` releases a single commit of `FinalDeployment` to every
instance and then checks each one is really running it.

```
git push origin FinalDeployment          ->  deploys ALL instances
Actions -> Deploy EIMS -> Run workflow   ->  pick one instance (or "all")
```

| Environment     | Site URL                         | Host                          | How it deploys |
|-----------------|----------------------------------|-------------------------------|----------------|
| `DMCPS`         | https://eims.mcpsbd.com          | Windows shared hosting (Plesk) | FTP, self-contained win-x64 |
| `Unityschoolbd` | https://eims.unityschoolbd.com   | Windows shared hosting (Plesk) | FTP, self-contained win-x64 |
| `Curiosity`     | https://curiosity.mcpsbd.com     | Windows shared hosting (Plesk) | FTP, self-contained win-x64 |
| `SchoolBD`      | https://schoolbd.hekzor.com      | Linux VPS (systemd + nginx)    | SSH → `eims-deploy.sh` builds on the box |

## What a run does

1. **Build** the self-contained `win-x64` package once (this also proves the commit compiles
   before any server is touched).
2. **Windows instances** (two at a time; one failing never blocks the others):
   - read the server's `web.config` and, if it still says `dotnet SMS_App.dll`, point
     `processPath` at `.\SMS_App.exe` — every other setting in it is kept;
   - upload `app_offline.htm`, mirror the package over FTP (only changed files after the
     first run), upload the patched `web.config` if needed, remove `app_offline.htm`.
3. **Linux instance**: SSH in and run the repo's `eims-deploy.sh` for the exact commit —
   it backs up the live release, builds, swaps, restarts `eims-noble.service` and health-checks.
4. **Verify**: request `<SITE_URL>/build-info` (returns `<version> <commit>`) and fail the job
   unless the instance reports the commit just released.

Self-contained Windows packages carry their own .NET runtime, so a host's installed runtime
version can never cause `HTTP 500.31` again.

## Never touched on a server

`appsettings*.json`, `Keys/`, `logs/`, `App_Data/` (online-admission uploads),
`wwwroot/Uploads/`, `wwwroot/Images/` (logos, student/employee photos). The FTP mirror
runs without `--delete`; the Linux rsync excludes the same paths.

## One-time setup

### 1. Environments
**Repo → Settings → Environments → New environment**, names exactly as in the table.
Remove the old `Mamitonnesa` environment — it is no longer deployed.

### 2. Secrets and variables per environment

| Name           | Kind     | Windows | Linux | Notes |
|----------------|----------|:-------:|:-----:|-------|
| `FTP_SERVER`   | secret   | ✔ |   | host only, no `ftp://` |
| `FTP_USERNAME` | secret   | ✔ |   | |
| `FTP_PASSWORD` | secret   | ✔ |   | |
| `FTP_PATH`     | secret   | ✔ |   | web root **as the FTP client sees it**: Plesk chrooted user → `/`, subscription-root user → `/httpdocs` |
| `SSH_HOST`     | secret   |   | ✔ | |
| `SSH_USER`     | secret   |   | ✔ | needs passwordless `sudo` (`root` works) |
| `SSH_KEY`      | secret   |   | ✔ | full private key (PEM) |
| `SITE_URL`     | variable | ✔ | ✔ | e.g. `https://eims.mcpsbd.com` — used by the verify step |

### 3. Runner
Jobs run only on a self-hosted runner carrying the label **`eims`** (`runs-on: [self-hosted, eims]`):
runner `eims-vps` on the SchoolBD VPS (62.84.181.246), installed in `/opt/cicd/runners/sms`,
running as user `github-runner` under systemd service
`actions.runner.golamhabibpalash-EIMS.eims-vps.service`.

The build uses the VPS's own .NET 8 SDK in `/usr/share/dotnet` (not `/usr/bin/dotnet`, which is
a separate .NET 10 install); keep an 8.0.x SDK installed there.

The VPS also hosts other production apps, so the service has a drop-in
(`/etc/systemd/system/<service>.d/limits.conf`) that keeps builds polite: `Nice=10`,
low IO priority, `CPUQuota=200%` (2 of 6 cores), `MemoryHigh=2500M`, `MemoryMax=3G`.

If the runner shows **Offline** in GitHub: `systemctl status actions.runner.golamhabibpalash-EIMS.eims-vps`.
If GitHub dropped the registration (it does after weeks offline), get a new token from
Settings → Actions → Runners → New self-hosted runner and, in `/opt/cicd/runners/sms`, run
`sudo -u github-runner ./config.sh --url https://github.com/golamhabibpalash/EIMS --token <token> --name eims-vps --labels eims --replace`.

## Releases that change the database

CI never changes a schema. When a release adds tables or permissions, run its scripts
from `Resources/sql/` (SqlServer or PostgreSQL version as appropriate) on **every**
instance **before** pushing. List them in the commit/PR so nothing is missed.

## Adding a school

1. Create its Environment with the secrets/variables above.
2. Add its name to `windows=(…)` or `linux=(…)` in the `setup` job and to the
   `workflow_dispatch` options in `deploy.yml`.

## Rollback

`git revert` the bad commit on `FinalDeployment` and push — the revert deploys like any
release. Linux also keeps the last 7 releases in `/var/www/eims-backups/`.
