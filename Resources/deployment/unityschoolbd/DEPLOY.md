# EIMS on Plesk Windows Shared Hosting (unityschoolbd.com)

Deploy **eims.unityschoolbd.com** with an MSSQL database. This guide assumes:

- Windows shared hosting running Plesk (IIS underneath).
- Main domain `unityschoolbd.com` is already added in Plesk.
- `.NET 8` SDK on your local machine (macOS works fine).

Everything below is already in this folder:
| File | Purpose |
|------|---------|
| `publish-windows.sh` | Builds a self-contained win-x64 package (no runtime needed on the server) |
| `appsettings.json.template` | Production config; fill in the encrypted DB connection string |
| `web.config` | IIS handler config, Production environment + stdout logging switch |
| `../EncryptConnString/` | Small tool to AES-encrypt the MSSQL connection string (same algorithm the app uses) |

---

## Phase 0 — Server prerequisites

1. Confirm with your host that IIS has the **ASP.NET Core Module v2** installed
   (Plesk Windows hosts with "ASP.NET" / ".NET Core" support have it). This is the
   only server-side requirement — the .NET 8 **runtime** is bundled in the
   self-contained publish.
2. In Plesk make sure **PHP/.NET** and **MSSQL** features are enabled for the subscription.

## Phase 1 — Create the subdomain

1. In Plesk: **Websites & Domains → Add Subdomain**.
   - Subdomain: `eims` (parent = unityschoolbd.com)
   - Keep the auto document root (usually `eims.unityschoolbd.com` with `httpdocs` inside).
2. Make sure `eims.unityschoolbd.com` resolves: add an **A record → your hosting IP**
   in your registrar/hosting DNS. Check with `nslookup eims.unityschoolbd.com`.

## Phase 2 — Create the MSSQL database + user

1. Plesk: **Databases → Add Database**.
   - Type: `Microsoft SQL Server`
   - Name: e.g. `eims_db` (Plesk may prefix it, e.g. `un_123_eims`).
2. In the same dialog create a new **database user** (login + password) and grant
   **all rights** (db_owner). Keep this password safe — it goes into the app config.
3. Note the **SQL Server hostname** Plesk shows (e.g. `db.lookehost.com` or an IP
   with instance name like `104.234.134.230\MSSQLSERVER2022`).

## Phase 3 — Build + configure on your machine

```bash
# 1. Publish self-contained win-x64
bash Resources/deployment/unityschoolbd/publish-windows.sh

# 2. Encrypt the connection string (adjust to your Plesk MSSQL details)
#    Pattern that is known to work: Server=<host>,1433
dotnet run --project Resources/deployment/EncryptConnString -- enc \
  "Server=<PLESK_MSSQL_HOST>,1433;Database=eims_db;User ID=<db_user>;Password=<db_pass>;Connection Timeout=15;TrustServerCertificate=True"
#    → prints a long base64 string. Save it.

# 3. In the publish folder: copy the template to appsettings.json and paste the value
cp Resources/deployment/unityschoolbd/appsettings.json.template \
   Resources/deployment/unityschoolbd/publish/appsettings.json
#   (edit publish/appsettings.json → "DefaultConnection": "<the base64 string>")

# 4. Replace the generated web.config with the production one
cp Resources/deployment/unityschoolbd/web.config \
   Resources/deployment/unityschoolbd/publish/web.config
```

### Connection string notes

- Use the exact **server name/instance** Plesk shows. Plesk MSSQL is usually
  reachable as `Server=<host>,1433` or `Server=<host>\MSSQLSERVER2022`.
- `TrustServerCertificate=True` avoids certificate validation pain on shared hosts.
- You can verify a value with:
  `dotnet run --project Resources/deployment/EncryptConnString -- dec "<base64>"`

## Phase 4 — Upload to Plesk

1. Plesk File Manager (or FTP) into the document root of `eims.unityschoolbd.com`
   → `httpdocs/`.
2. **Optional but recommended:** upload an `app_offline.htm` file first
   (`<html>maintenance</html>`) so IIS stops the app while files are copied.
3. Upload **all contents** of `Resources/deployment/unityschoolbd/publish/`
   (including `wwwroot`, `web.config`, `appsettings.json`, all `.dll`/`.exe`).
4. Delete `app_offline.htm` when the upload finishes.
5. Ensure the app pool user can **write** to these folders (Plesk normally does this
   automatically, but check if seeding/startup fails):
   - the site root (so `Keys/` can be created for data protection)
   - `wwwroot` and its subfolders (student/employee photo uploads)

## Phase 5 — Start & verify

1. Open **https://eims.unityschoolbd.com**. First hit may take 10–30 seconds while:
   - the app connects to MSSQL,
   - `EnsureCreated()` builds the **full schema automatically** (no migrations needed on a fresh DB),
   - the seeder creates **SuperAdmin** role + admin account.
2. Login with the seeded account and **change the password immediately**:
   - Email: `admin@eims.com`
   - Password: `Admin@123`
3. Enable **Let's Encrypt** in Plesk for `eims.unityschoolbd.com` (HTTPS).
   The app already handles `X-Forwarded-Proto` and `UseHttpsRedirection()`.

### If it won't start

- Flip `stdoutLogEnabled` to `true` in `httpdocs/web.config`, ensure the
  `httpdocs/logs` folder is writable, reload the page, then read `httpdocs/logs/stdout_*.log`.
- Check the DB user has rights to create tables (db_owner).
- Confirm `DatabaseProvider` in `appsettings.json` is exactly `"SqlServer"` and the
  `DefaultConnection` value really was produced by the encrypt tool above.

---

## Rules of thumb

- **Never upload another school's `appsettings.json`.** Only the Unity version.
- **Back up `httpdocs/Keys/`** (data-protection keys). Lose them and existing logins break.
- Re-apply the tuned `web.config` after every `dotnet publish` (it gets regenerated).
- `Hangfire` is disabled by default — keep it off on shared hosting.
- Seeding only runs when the admin account does not exist, so it is safe to re-deploy
  without losing data.