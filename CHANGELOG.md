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

**Users see these notes** in the app's Version dialog ("What's new in this version"); the
section for the running version is read from this file, which is embedded in SMS_App.dll.
So write each entry for school staff, not developers: what they can now do or what no
longer goes wrong, naming the screen. Rules the dialog relies on:

- Version headings are `## [x.y.z] - yyyy-mm-dd`; groups are `### Added` (shown as "New
  features"), `### Fixed` ("Bug fixes"), `### Changed` ("Improvements").
- One `- ` bullet per task; wrap onto lines indented by two spaces. `**bold**` is allowed.
- Entries starting with `CI` are internal (deploy pipeline) and are hidden from users.

## [Unreleased]

## [1.3.0] - 2026-11-09

### Added
- **Free fees for a student**: on Student Fee Allocation you can now set an amount of 0
  for a fee in a chosen session, and that fee is no longer charged to the student.
- **Safer allocation delete**: an allocation can no longer be deleted once the student has
  paid against that fee; the delete window explains why and suggests deactivating it.
- **Permission-based fee allocation access**: Edit and Delete buttons appear only for users
  who have those permissions, and any role given the Fee Allocation permissions can use it.
- **What's new in this version**: the Version window now lists the tasks done in the
  running version instead of only technical file details.

### Fixed
- **Employee photos**: photos on the Employees page are no longer squashed or stretched.
- **Allocation session**: a new fee allocation is now saved for the session you selected,
  not always the student's current session.
- **Fee allocation alerts**: success and error messages now appear on the Fee Allocation
  page (for example "already allocated").
- **Group fee allocation**: no longer shows an error page when the class fee isn't set up
  for that session; it tells you to add the class fee first. Negative amounts are rejected.
- **Payment fee amounts**: the Payment page no longer mixes in allocations from other
  sessions or inactive allocations.
- **Fee allocation data security**: the allocation list can no longer be read without
  signing in.
- **Institute logo update**: updating the logo, banner or title icon on Institute
  information no longer fails silently. The file type and size (up to 5 MB) are checked,
  and if the server can't save the image, the page says why.
- **Institute Edit page clearing data**: saving from the Institute Edit page no longer
  erases the short name, phone numbers, email and school times.
- **Institute Edit page error**: opening the Institute Edit page without an institute
  selected no longer shows an error page.
- **Error alerts on many screens**: "failed" messages that were never displayed (on about
  30 screens) now appear as red alerts.
- CI: FTP retries never ran (the first failure ended the step under bash -e).

### Changed
- **Footer**: now reads "SchoolBD! Developed By HEKZOR", with HEKZOR linking to hekzor.com.
- **Institute Edit page**: redesigned into clear General, Contact, School time and Images
  sections, with a preview of the current and newly chosen logo, banner and title icon.
- **Version window**: technical build details are tucked under "Technical details".
- CI: Windows deploys upload only changed files (checksum manifest), with a full_sync
  option on manual runs; deploy jobs read the build from the runner's disk.
