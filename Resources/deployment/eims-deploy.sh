#!/bin/bash
# ============================================================
# EIMS single-instance deploy  (schoolbd.hekzor.com)
# Pulls FinalDeployment, builds with the .NET 8 SDK, and swaps
# the published output into the live app dir, preserving config,
# data-protection keys and uploaded images. Then restarts & checks.
#
#   Usage:  sudo /usr/local/bin/eims-deploy        # deploy latest FinalDeployment
#           sudo /usr/local/bin/eims-deploy <ref>  # deploy a specific commit/tag/branch
#
# Safe to re-run. Backs up the current release before swapping.
# ============================================================
set -euo pipefail

# ---- CONFIG ----
REPO="https://github.com/golamhabibpalash/EIMS.git"
BRANCH="${1:-FinalDeployment}"
SRC_DIR="/var/www/eims-deploy"          # dedicated checkout (NOT eims-src)
APP_DIR="/var/www/eims"                 # live published app
SERVICE="eims-noble.service"            # serves schoolbd.hekzor.com on :5001
BACKUP_DIR="/var/www/eims-backups"
HEALTH_URL="http://127.0.0.1:5001"
DOTNET_ROOT="/usr/share/dotnet"         # has SDK 8.0.x (global.json pins 8.0)
DOTNET="$DOTNET_ROOT/dotnet"
APP_USER="www-data"
APP_GROUP="www-data"

export DOTNET_ROOT
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

log() { echo -e "\033[0;32m[deploy]\033[0m $*"; }
err() { echo -e "\033[0;31m[deploy]\033[0m $*" >&2; }

[ "$(id -u)" -eq 0 ] || { err "run as root (sudo)"; exit 1; }
[ -x "$DOTNET" ]     || { err "no dotnet 8 SDK at $DOTNET"; exit 1; }

# ---- 1. Get the source at the requested ref ----
if [ ! -d "$SRC_DIR/.git" ]; then
    log "cloning $REPO -> $SRC_DIR"
    git clone "$REPO" "$SRC_DIR"
fi
cd "$SRC_DIR"
log "fetching origin and resetting to $BRANCH"
git fetch --prune origin
git checkout -B "$BRANCH" "origin/$BRANCH"
git reset --hard "origin/$BRANCH"
COMMIT="$(git rev-parse --short HEAD)"
log "building commit $COMMIT"

# ---- 2. Publish (framework-dependent, net8.0) ----
PUB_DIR="$SRC_DIR/publish"
rm -rf "$PUB_DIR"
"$DOTNET" publish SMS_App/SMS_App.csproj -c Release -o "$PUB_DIR" --nologo \
    -p:GitCommit="$COMMIT" \
    -p:BuildTimestampUtc="$(date -u '+%Y-%m-%d %H:%M:%S')"

# Never ship repo config / symbols over the server's own files.
rm -f "$PUB_DIR"/appsettings*.json "$PUB_DIR"/*.pdb
log "publish complete"

# ---- 3. Back up the current live release (app only, not images) ----
mkdir -p "$BACKUP_DIR"
STAMP="$(date '+%Y%m%d-%H%M%S')"
BACKUP="$BACKUP_DIR/eims-$STAMP-pre-$COMMIT.tar.gz"
log "backing up current release -> $BACKUP"
tar --warning=no-file-changed -czf "$BACKUP" \
    --exclude="./wwwroot/Images" --exclude="./logs" -C "$APP_DIR" . || true
# keep only the 7 most recent backups
ls -1t "$BACKUP_DIR"/eims-*.tar.gz 2>/dev/null | tail -n +8 | xargs -r rm -f

# ---- 4. Swap in the new build (preserve config/keys/logs/images) ----
log "stopping $SERVICE"
systemctl stop "$SERVICE"

log "syncing new build into $APP_DIR"
rsync -a --delete \
    --exclude="appsettings*.json" \
    --exclude="/Keys/" \
    --exclude="/logs/" \
    --exclude="/wwwroot/Images/" \
    "$PUB_DIR"/ "$APP_DIR"/

chown -R "$APP_USER:$APP_GROUP" "$APP_DIR"

log "starting $SERVICE"
systemctl start "$SERVICE"

# ---- 5. Health check ----
log "health check $HEALTH_URL"
ok=0
for i in $(seq 1 10); do
    code="$(curl -s -o /dev/null -w '%{http_code}' -L --max-time 10 "$HEALTH_URL" || echo 000)"
    echo "  attempt $i: HTTP $code"
    case "$code" in 200|301|302) ok=1; break;; esac
    sleep 3
done

if [ "$ok" -eq 1 ]; then
    log "DONE - deployed $COMMIT, $SERVICE is up."
else
    err "service did not answer with 2xx/3xx. Check: journalctl -u $SERVICE -n 50"
    err "rollback: tar -xzf $BACKUP -C $APP_DIR && systemctl restart $SERVICE"
    exit 1
fi
