#!/bin/bash
# ============================================================================
# Onboard a NEW school on the SchoolBD platform (VPS). Idempotent: safe to re-run.
#
#   sudo bash new-school.sh <code> "<School name>" <admin-email>
#   e.g. sudo bash new-school.sh demo "Demo School" admin@demo.schoolbd.net
#
# Creates on the EIMS host PostgreSQL (port 5433): role sbd_<code>_app + database sbd_<code>,
# registers the school, creates its schema + admin account, adds <code>.schoolbd.net and
# activates it. Generated passwords are written ONLY to /etc/schoolbd/schools/<code>.txt (root, 600).
# ============================================================================
set -euo pipefail

CODE="${1:?school code (lowercase, e.g. abc)}"
NAME="${2:?school name}"
ADMIN_EMAIL="${3:?admin email}"
APP_DIR="/var/www/schoolbd-platform"
ENV_FILE="/etc/schoolbd/platform.env"
SCHOOLS="/etc/schoolbd/schools"
PG_PORT=5433
SERVER_NAME="VPS-PG5433"
ROLE="sbd_${CODE//-/_}_app"
DB="sbd_${CODE//-/_}"

die() { echo "[new-school] $*" >&2; exit 1; }
[ "$(id -u)" -eq 0 ] || die "run as root"
[[ "$CODE" =~ ^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?$ ]] || die "code must be lowercase letters/digits/-"

install -d -m 700 "$SCHOOLS"
SECRET_FILE="$SCHOOLS/$CODE.txt"
if [ ! -f "$SECRET_FILE" ]; then
    umask 077
    printf 'DB_PASSWORD=%s\nADMIN_EMAIL=%s\nADMIN_PASSWORD=%s\n' "$(openssl rand -hex 24)" "$ADMIN_EMAIL" "$(openssl rand -base64 12 | tr -d '/+=')A1" > "$SECRET_FILE"
    umask 022
fi
# shellcheck disable=SC1090
. "$SECRET_FILE"

# Database + login for this school only.
psql_admin() { sudo -u postgres psql -p "$PG_PORT" -v ON_ERROR_STOP=1 -qtA "$@"; }
psql_admin -c "select 1 from pg_roles where rolname='$ROLE'" | grep -q 1 \
    || psql_admin -c "CREATE ROLE $ROLE LOGIN PASSWORD '$DB_PASSWORD';"
psql_admin -c "select 1 from pg_database where datname='$DB'" | grep -q 1 \
    || psql_admin -c "CREATE DATABASE $DB OWNER $ROLE;"
psql_admin -c "REVOKE CONNECT ON DATABASE $DB FROM PUBLIC;"

cd "$APP_DIR"
set -a; . "$ENV_FILE"; set +a
export SBD_DB_PW="$DB_PASSWORD" SBD_ADMIN_PW="$ADMIN_PASSWORD"
p() { echo "\$ platform $*" | sed -E 's/(--[a-z-]*password-env) [A-Z_]+/\1 ***/'; sudo -E -u www-data /usr/bin/dotnet SMS_App.dll platform "$@" 2>&1 | grep -vE '^\s*(info|warn|dbug|fail):|^\s{6}'; }

p server-add  --name "$SERVER_NAME" --provider PostgreSQL --host 127.0.0.1 --port "$PG_PORT"
p tenant-add  --code "$CODE" --name "$NAME"
p tenant-db   --code "$CODE" --server "$SERVER_NAME" --database "$DB" --user "$ROLE" --password-env SBD_DB_PW
p tenant-init --code "$CODE" --admin-email "$ADMIN_EMAIL" --admin-password-env SBD_ADMIN_PW
p domain-add  --code "$CODE" --domain "$CODE.schoolbd.net" --primary
p tenant-status --code "$CODE" --status Active
echo "[new-school] done. Admin login is in $SECRET_FILE (root only)."
