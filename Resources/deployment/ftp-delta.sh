#!/bin/bash
# Checksum-based ("delta") FTP release for the Windows (IIS/Plesk) instances.
#   source Resources/deployment/ftp-lib.sh
#   source Resources/deployment/ftp-delta.sh
#
# Why: the hosting FTP server can't keep timestamps, so `lftp mirror` has to list and
# size-compare all ~3,000 remote files on every release, which is most of the deploy time.
# Instead, each release leaves a manifest (sha256 + path of every file it shipped) in the
# site root. The next release diffs its own manifest against that one and uploads only
# what changed - usually SMS*.dll plus a few js/css files - without listing the server.
#
# Safety rules:
#  - The server manifest is written LAST, only after every file was uploaded. A release
#    that fails part-way leaves the old manifest, so the next run resends those files.
#  - No manifest on the server, an unreadable one, a huge change set, or a manual
#    "full sync" falls back to the full `mirror` path (the caller decides; see delta_plan).
#  - Files are never deleted from the server, same as mirror.
#
# The manifest lives in the site root, which ASP.NET Core never serves (only wwwroot is).

DELTA_MANIFEST_NAME=".eims-deploy-manifest.txt"
DELTA_MAX_FILES=400   # above this, one mirror pass is faster than per-file puts

# delta_manifest <dir> <out-file>
# Writes "<sha256>  <relative/path>" for every file under <dir>, sorted by path.
delta_manifest() {
    local dir="$1" out="$2"
    ( cd "$dir" && find . -type f -print0 | LC_ALL=C sort -z | xargs -0 sha256sum ) \
        | sed 's#  \./#  #' > "$out"
}

# delta_fetch_server_manifest <out-file>
# Downloads the manifest of the release currently on the server. Returns 1 if there is none.
delta_fetch_server_manifest() {
    local out="$1"
    rm -f "$out"
    ftp_session "get $DELTA_MANIFEST_NAME -o $out" 90 >/dev/null 2>&1 || { rm -f "$out"; return 1; }
    # Every line must look like "<64 hex>  <path>"; anything else means we can't trust it.
    [ -s "$out" ] && ! grep -qvE '^[0-9a-f]{64}  .+$' "$out"
}

# delta_plan <new-manifest> <old-manifest> <online-list> <offline-list>
# Splits the changed files into:
#   online  - safe to upload while the site is serving: anything under wwwroot/ (static
#             files IIS never locks) and files that are NEW on the server (the running app
#             can't have them loaded);
#   offline - existing files outside wwwroot whose content changed (DLLs IIS has locked).
delta_plan() {
    local new="$1" old="$2" online="$3" offline="$4"
    awk -v online="$online" -v offline="$offline" '
        # sha256sum lines are "<64 hex><2 spaces><path>", so the path starts at column 67.
        NR == FNR { seen[substr($0, 67)] = substr($0, 1, 64); next }
        {
            hash = substr($0, 1, 64); path = substr($0, 67)
            if (path in seen && seen[path] == hash) next
            if (path ~ /^wwwroot\// || !(path in seen)) print path > online
            else print path > offline
        }' "$old" "$new"
    touch "$online" "$offline"
}

# delta_upload <local-root> <list-file> <label>
# Uploads every path in <list-file> from <local-root> to the same relative path on the
# server, creating folders first. Two sessions run in parallel (the host allows ~2), and
# each is retried as a whole; re-uploading an already-sent file is harmless.
delta_upload() {
    local root="$1" list="$2" label="$3" n
    n=$(grep -c . "$list" || true)
    if [ "${n:-0}" -eq 0 ]; then
        echo "$label: nothing to upload."
        return 0
    fi
    echo "$label: uploading $n file(s):"
    sed 's/^/  /' "$list"

    local dirs_cmds
    dirs_cmds="$(sed -n 's#/[^/]*$##p' "$list" | LC_ALL=C sort -u | sed 's/.*/mkdir -p "&"/')"
    if [ -n "$dirs_cmds" ]; then
        # An existing folder makes mkdir fail; that is expected, so don't stop on it.
        ftp_retry 3 300 "set cmd:fail-exit no
$dirs_cmds
set cmd:fail-exit yes" || return 1
    fi

    local tmp; tmp="$(mktemp -d)"
    split -n "l/2" "$list" "$tmp/part-"
    local part pids=() rc=0 limit=$(( 300 + n * 20 ))
    for part in "$tmp"/part-*; do
        [ -s "$part" ] || continue
        local cmds
        cmds="lcd \"$root\"
$(sed 's/.*/put "&" -o "&"/' "$part")"
        ftp_retry 4 "$limit" "$cmds" &
        pids+=($!)
    done
    for p in "${pids[@]}"; do wait "$p" || rc=1; done
    rm -rf "$tmp"
    return $rc
}

# delta_record <manifest>
# Stores the manifest of the release that is now fully on the server.
delta_record() {
    local manifest="$1"
    ftp_retry 3 120 "put \"$manifest\" -o $DELTA_MANIFEST_NAME"
}
