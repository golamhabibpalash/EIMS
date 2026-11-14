#!/bin/bash
# ============================================================================
# SchoolBD multi-tenant platform: first-time install / re-install on the Linux VPS.
# Additive and idempotent: creates its own directories, database, env file and
# systemd service. It never modifies other services, nginx sites or databases.
#
#   sudo bash install-platform.sh [publish_dir]
#
# publish_dir defaults to /var/www/eims-deploy/publish (left by eims-deploy.sh for the
# last FinalDeployment release). Secrets are generated once into /etc/schoolbd/platform.env
# (root, 600) and never printed.
# ============================================================================
set -euo pipefail

PUB_DIR="${1:-/var/www/eims-deploy/publish}"
APP_DIR="/var/www/schoolbd-platform"
STORAGE="/var/lib/schoolbd/tenants"
ETC="/etc/schoolbd"
ENV_FILE="$ETC/platform.env"
SERVICE="schoolbd-platform"
PORT=5100
PG_PORT=5433                     # host PostgreSQL 16 that already hosts the EIMS databases
PG_DB="SchoolBD_PlatformDB"
PG_ROLE="schoolbd_platform"
APP_USER="www-data"

log() { echo -e "\033[0;32m[platform]\033[0m $*"; }
die() { echo -e "\033[0;31m[platform]\033[0m $*" >&2; exit 1; }
[ "$(id -u)" -eq 0 ] || die "run as root"
[ -f "$PUB_DIR/SMS_App.dll" ] || die "no build at $PUB_DIR"

# ---- 1. Directories ---------------------------------------------------------
install -d -m 700 -o root -g root "$ETC"
install -d -o "$APP_USER" -g "$APP_USER" "$APP_DIR" "$STORAGE"

# ---- 2. Secrets (generated once, kept on re-runs) ---------------------------
if [ ! -f "$ENV_FILE" ]; then
    log "generating $ENV_FILE"
    PG_PW="$(openssl rand -hex 24)"
    umask 077
    cat > "$ENV_FILE" <<EOF
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://127.0.0.1:$PORT
DOTNET_CLI_TELEMETRY_OPTOUT=1
MultiTenancy__Enabled=true
MultiTenancy__PlatformRootDomain=schoolbd.net
MultiTenancy__StorageRoot=$STORAGE
MultiTenancy__ResolutionCacheSeconds=60
MultiTenancy__Platform__Provider=PostgreSQL
# Quoted: the value contains ";" (both systemd and `set -a; . file` accept double quotes).
MultiTenancy__Platform__ConnectionString="Host=127.0.0.1;Port=$PG_PORT;Database=$PG_DB;Username=$PG_ROLE;Password=$PG_PW"
SCHOOLBD_SECRETS_KEY=$(openssl rand -base64 32)
Jwt__Key=$(openssl rand -base64 48 | tr -d '\n')
Jwt__Issuer=EIMS
Jwt__Audience=EIMS.Mobile
Hangfire__IsEnabled=false
EOF
    chmod 600 "$ENV_FILE"
    umask 022
else
    log "$ENV_FILE exists, keeping it"
fi
PG_PW="$(grep -oP '(?<=Password=)[^;"]+' "$ENV_FILE" | head -1)"

# ---- 3. Platform database (new role + database only) -------------------------
log "ensuring PostgreSQL role $PG_ROLE and database $PG_DB on port $PG_PORT"
if ! sudo -u postgres psql -p "$PG_PORT" -tAc "select 1 from pg_roles where rolname='$PG_ROLE'" | grep -q 1; then
    sudo -u postgres psql -p "$PG_PORT" -v ON_ERROR_STOP=1 -q -c "CREATE ROLE $PG_ROLE LOGIN PASSWORD '$PG_PW';"
fi
if ! sudo -u postgres psql -p "$PG_PORT" -tAc "select 1 from pg_database where datname='$PG_DB'" | grep -q 1; then
    sudo -u postgres psql -p "$PG_PORT" -v ON_ERROR_STOP=1 -q -c "CREATE DATABASE \"$PG_DB\" OWNER $PG_ROLE;"
fi

# ---- 4. App files -------------------------------------------------------------
log "syncing build from $PUB_DIR"
rsync -a --delete \
    --exclude="appsettings*.json" --exclude="/Keys/" --exclude="/logs/" \
    --exclude="/App_Data/" --exclude="/wwwroot/Uploads/" \
    "$PUB_DIR"/ "$APP_DIR"/
if [ ! -f "$APP_DIR/appsettings.json" ]; then
    # Minimal: every secret comes from the env file. No DefaultConnection, no SMS token
    # (schools on the platform need their own SMS settings before SMS is switched on).
    cat > "$APP_DIR/appsettings.json" <<'EOF'
{
  "DatabaseProvider": "PostgreSQL",
  "Logging": { "LogLevel": { "Default": "Warning", "Microsoft.AspNetCore": "Warning", "Microsoft.EntityFrameworkCore": "Error" } },
  "AllowedHosts": "*",
  "Hangfire": { "IsEnabled": false }
}
EOF
fi
chown -R "$APP_USER:$APP_USER" "$APP_DIR"

# ---- 5. systemd service --------------------------------------------------------
cat > "/etc/systemd/system/$SERVICE.service" <<EOF
[Unit]
Description=SchoolBD multi-tenant platform (*.schoolbd.net)
After=network.target postgresql.service

[Service]
Type=simple
WorkingDirectory=$APP_DIR
ExecStart=/usr/bin/dotnet $APP_DIR/SMS_App.dll
EnvironmentFile=$ENV_FILE
Restart=always
RestartSec=10
User=$APP_USER
Group=$APP_USER
# Same resource etiquette as the other apps on this shared VPS.
CPUQuota=150%
MemoryMax=1500M

[Install]
WantedBy=multi-user.target
EOF
systemctl daemon-reload
systemctl enable "$SERVICE" >/dev/null 2>&1

# ---- 6. Platform schema (idempotent) then (re)start ----------------------------
log "creating platform tables (if missing)"
( cd "$APP_DIR" && set -a && . "$ENV_FILE" && set +a && sudo -E -u "$APP_USER" /usr/bin/dotnet SMS_App.dll platform init )

systemctl restart "$SERVICE"
for i in $(seq 1 20); do
    code="$(curl -s -o /dev/null -w '%{http_code}' --max-time 5 "http://127.0.0.1:$PORT/health" || true)"
    [ "$code" = "200" ] && { log "UP: $(curl -s http://127.0.0.1:$PORT/build-info) - /health ok"; exit 0; }
    sleep 3
done
die "not healthy; see: journalctl -u $SERVICE -n 50"
