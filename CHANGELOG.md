# Changelog

All notable changes to EIMS. The version is `VersionPrefix` in `Directory.Build.props`;
CI appends the build number (e.g. `1.3.0.57`), so this file tracks product versions only.

Versioning follows [Semantic Versioning](https://semver.org/):

- **MAJOR** (`2.0.0`) — breaking change: a schema script must be run before deploying, a
  removed/renamed screen or API, or a change in behaviour users must be told about.
- **MINOR** (`1.4.0`) — new feature or capability, backwards compatible.
- **PATCH** (`1.3.1`) — bug fixes, UI polish, performance, CI/deploy changes only.

Every change that ships adds an entry under **Unreleased**; when the release is cut, the
heading becomes the new version and `VersionPrefix` is bumped to match.

## [Unreleased]

## [1.3.0] - 2026-11-09

### Added
- **Fee allocation — free (0) amount**: a student can be given a 0 allocation for a fee
  head in a chosen session; the Payment page and dues now treat it as free.
- **Fee allocation — safe delete**: delete is blocked once payments exist for that fee in
  that session (suggests deactivating instead). The delete dialog checks first and shows
  the reason; every outcome is shown as an alert.
- **Fee allocation — permission-based access**: Edit/Delete buttons appear only for users
  with "Edit/Delete Student Fee Allocations"; the page no longer requires the Admin or
  SuperAdmin role, only its own permissions.
- **CI/CD**: Windows deploys upload only changed files (checksum manifest), with a
  `full_sync` option on manual runs.

### Fixed
- Employees page: photos were squashed/stretched; now a proportional circular crop. The
  fallback avatar path broke on Linux (case-sensitive `/images/`).
- Fee allocation create used the student's current session instead of the selected one.
- Fee allocation list data endpoint was reachable without login and leaked stack traces.
- Group fee allocation: added anti-forgery validation; no longer crashes when the class
  fee isn't set up for the session; rejects negative amounts.
- Payment fee list summed allocations across all sessions, including inactive ones.
- Fee allocation page never showed its success/error messages.
- CI: FTP retries never ran (the first failure ended the step under `bash -e`).

### Changed
- CI: deploy jobs read the build from the runner's disk instead of GitHub artifacts.
- Footer branding: "SchoolBD! Developed By HEKZOR", with HEKZOR linking to
  http://hekzor.com/ (opens in a new tab).
