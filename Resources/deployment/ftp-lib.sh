#!/bin/bash
# FTP helpers for the Windows (IIS/Plesk) deploy steps in .github/workflows/deploy.yml.
#   source Resources/deployment/ftp-lib.sh
#   ftp_retry <attempts> <seconds-per-attempt> "<lftp commands>"
#
# Needs env: FTP_SERVER  FTP_USERNAME  LFTP_PASSWORD (read by lftp --env-password, so the
# password never shows in `ps`)  FTP_PATH (the folder that holds SMS_App.dll as seen after
# FTP login; "/" when the account opens straight into the site folder).
#
# Shared hosting stalls or rate-limits FTP connections, so every session:
#  - stops at the first failing command (a bad FTP_PATH can't upload into the wrong folder),
#  - uses at most 2 parallel transfers (callers pass --parallel=2),
#  - is killed after <seconds-per-attempt> if the connection hangs, and
#  - is retried; `mirror` only sends what is still missing/different, so a retry resumes.

ftp_session() {
    local cmds="$1" limit="$2"
    timeout --kill-after=30 "$limit" lftp --env-password -u "$FTP_USERNAME" "ftp://$FTP_SERVER" <<EOF
set cmd:fail-exit yes
set ftp:passive-mode on
set ssl:verify-certificate no
set net:timeout 30
set net:max-retries 3
set net:reconnect-interval-base 10
set net:reconnect-interval-multiplier 1
set ftp:use-site-chmod no
set ftp:use-site-utime no
set file:charset utf-8
cd "$FTP_PATH"
$cmds
bye
EOF
}

ftp_retry() {
    local attempts="$1" limit="$2" cmds="$3" i rc why
    for i in $(seq 1 "$attempts"); do
        ftp_session "$cmds" "$limit"; rc=$?
        [ "$rc" -eq 0 ] && return 0
        if [ "$i" -lt "$attempts" ]; then
            [ "$rc" -eq 124 ] && why="timed out" || why="exit $rc"
            echo "::warning::FTP attempt $i/$attempts failed ($why); resuming in 30s"
            sleep 30
        fi
    done
    echo "::error::FTP still failing after $attempts attempts"
    return 1
}
