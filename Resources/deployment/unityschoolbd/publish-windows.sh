#!/bin/bash
# ============================================================
# Publish EIMS for unityschoolbd.com — Windows shared hosting (Plesk + IIS)
# Self-contained win-x64 so the server does NOT need the .NET 8 runtime installed.
# Run from your Mac:  bash Resources/deployment/unityschoolbd/publish-windows.sh
# ============================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
RELEASE_DIR="$SCRIPT_DIR/publish"

BUILD_NUMBER="${BUILD_NUMBER:-0}"
GIT_COMMIT="${GIT_COMMIT:-local}"
BUILT_UTC="$(date -u '+%Y-%m-%d %H:%M:%S')"

echo "[INFO] Repo root : $REPO_ROOT"
echo "[INFO] Output    : $RELEASE_DIR"
echo "[INFO] Build     : $GIT_COMMIT @ $BUILT_UTC"

rm -rf "$RELEASE_DIR"

dotnet publish "$REPO_ROOT/SMS_App/SMS_App.csproj" \
    --configuration Release \
    --output "$RELEASE_DIR" \
    --runtime win-x64 \
    --self-contained true \
    -p:PublishSingleFile=false \
    -p:PublishTrimmed=false \
    -p:BuildNumber="$BUILD_NUMBER" \
    -p:GitCommit="$GIT_COMMIT" \
    -p:BuildTimestampUtc="$BUILT_UTC"

echo ""
echo "[OK] Publish complete. Next steps:"
echo "   1. Create the MSSQL DB + user in Plesk (see DEPLOY.md)"
echo "   2. Encrypt the connection string:"
echo "        dotnet run --project Resources/deployment/EncryptConnString -- enc \"Server=...;Database=...;User ID=...;Password=...\""
echo "   3. Put the encrypted value into publish/appsettings.json"
echo "   4. Upload publish/ contents to eims.unityschoolbd.com httpdocs"