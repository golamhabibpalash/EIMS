#!/bin/bash
# Confirms an instance is serving the build that was just deployed.
#   Usage: verify-build.sh <site-url> <expected-commit>
# <site-url>/build-info returns "<version> <short-commit>". The expected commit may be
# short or full; it matches when one is a prefix of the other. Exits 1 on mismatch.
set -uo pipefail

SITE_URL="${1:-}"
EXPECTED="${2:-}"

if [ -z "$SITE_URL" ]; then
    echo "::warning::No SITE_URL variable for this environment - cannot verify the deploy."
    exit 0
fi

url="${SITE_URL%/}/build-info"
for i in $(seq 1 12); do
    body="$(curl -s -L --max-time 20 "$url" || true)"
    code="$(curl -s -o /dev/null -L --max-time 20 -w '%{http_code}' "$url" || echo 000)"
    live="$(echo "$body" | awk '{print $2}')"
    echo "Attempt $i: HTTP $code, live build: ${body:-<none>}"
    if [ "$code" = "200" ] && [ -n "$live" ] && { [[ "$EXPECTED" == "$live"* ]] || [[ "$live" == "$EXPECTED"* ]]; }; then
        echo "OK - $SITE_URL is running $live"
        exit 0
    fi
    sleep 10   # first request after a deploy starts the app, which takes a while
done

echo "::error::$SITE_URL is not running commit $EXPECTED (last answer: HTTP $code '${body}')"
exit 1
