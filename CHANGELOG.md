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

## [2.0.0] - 2026-11-16

### Added
- Students: Create, Edit, Details and Profile now have Father's Name (Bangla), Mother's
  Name (Bangla), and an upload for the father's and mother's NID (JPG, PNG or PDF, up to
  2 MB). All are optional. NID copies are kept private and open only for staff who can view
  the student, or for the student themselves. Students admitted through Online Admission
  bring their parents' Bangla names with them.

### Changed
- CI Needs Resources/sql/Student_ParentInfo_SqlServer.sql (or _PostgreSQL.sql, on every
  school database) run before this release is deployed.

### Fixed
- Adding a student now keeps the uploaded photo and birth certificate image; before, the
  files were uploaded but not linked to the student.
- Editing a student no longer removes their birth certificate image.
- The Guardian's Phone now shows on the student's Details page.
- The verification code screen (Forgot Password) is easier to use: a large full-width code
  box that opens the number keypad on phones and can fill the code from the SMS, a clear
  Verify button, a note saying where the code was sent, and a link to send a new code.
- Testimonial and Transfer Certificate now print on a single page. The issue date and
  signature no longer move onto a second page, and a certificate with very long names or
  address is shrunk slightly to fit instead of being split.

## [1.6.1] - 2026-11-15

### Fixed
- A newly set-up school no longer opens with an empty menu. Setting up a school now also
  creates its modules and permission list and gives the school's admin every permission,
  so all menus show on first login. Running setup again on an existing school adds only
  what is missing.

## [1.6.0] - 2026-11-15

### Added
- **Due by date range on Due Payment**: you can now choose a fee period (From and To dates)
  to see who owes for those months only, for example "Up to this month" or "March to May".
  Quick buttons fill in This month, Up to this month, Last 3 months or Whole session. Leave
  the dates empty to see the whole session as before.
- **Sort, search and export on Due Payment**: click any column heading to sort, search by
  name, roll, ID or phone, and download the list as Excel, CSV or PDF in the same order and
  with the same search you see on screen. **Print** prints every matching student.
- **More detail on Due Payment**: each student now shows unique ID, class, contact number,
  status, and payable and paid next to the due, with totals that update as you search. You
  can also pick the session, and a switch (on by default) hides students with nothing to pay.

### Fixed
- **Due Payment (Previous)**: the **Get list** button was cut off on wide screens.

## [1.5.0] - 2026-11-15

### Added
- **Sort and export on Due Payment (Previous)**: click any column heading to sort the due
  list, and use the search box to find a student by name, roll, ID or phone. The new
  **Export** button downloads the list as Excel, CSV or PDF in the same order and with the
  same search you see on screen, and **Print** prints every matching student.
- **More detail on Due Payment (Previous)**: the list now shows each student's unique ID,
  class, contact number, status, and total payable and paid next to the due amount, with
  totals at the top and bottom that update as you search.
- **Only students with a due**: a new switch (on by default) hides students who have
  nothing left to pay; turn it off to see everyone.

### Fixed
- **Inactive students on Due Payment (Previous)**: choosing status "Inactive" now lists
  inactive students' dues instead of always showing nothing.
- **Report heading on Due Payment (Previous)**: the heading now names the class, section
  and student you chose instead of always saying "All Class".

## [1.4.0] - 2026-11-14

### Added
- **One installation for many schools**: SchoolBD can now run many schools from a single
  installation. Each school has its own web address (for example abc.schoolbd.net, or the
  school's own domain) and its own separate database, so no school can see another school's
  students, payments, photos or logins. Existing school sites keep working exactly as before.

### Fixed
- **Page titles mixing between users**: when several people used the system at the same
  time, a page could show the title of a page someone else had just opened. Each page now
  always shows its own title.

### Changed
- CI: added an automated test project (SMS.Tests) for school-isolation checks; it is not
  part of the deployed package.

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
