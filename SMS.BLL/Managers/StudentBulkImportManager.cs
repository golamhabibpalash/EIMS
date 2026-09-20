using ClosedXML.Excel;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS.Entities.AdditionalModels.StudentImport;
using SMS.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    /// <summary>
    /// Reads a .csv/.xlsx roster, resolves every lookup column by name and validates
    /// each row against the same rules the single-student Create screen applies.
    /// Parsing and committing are deliberately separate so the user always sees a
    /// preview before anything is written.
    /// </summary>
    public class StudentBulkImportManager : IStudentBulkImportManager
    {
        private readonly IStudentManager _studentManager;
        private readonly IAcademicClassManager _academicClassManager;
        private readonly IAcademicSessionManager _academicSessionManager;
        private readonly IAcademicSectionManager _academicSectionManager;
        private readonly IReligionManager _religionManager;
        private readonly IGenderManager _genderManager;
        private readonly INationalityManager _nationalityManager;
        private readonly IBloodGroupManager _bloodGroupManager;
        private readonly IDivisionManager _divisionManager;
        private readonly IDistrictManager _districtManager;
        private readonly IUpazilaManager _upazilaManager;
        private readonly IStudentActivateHistManager _studentActivateHistManager;

        public StudentBulkImportManager(
            IStudentManager studentManager,
            IAcademicClassManager academicClassManager,
            IAcademicSessionManager academicSessionManager,
            IAcademicSectionManager academicSectionManager,
            IReligionManager religionManager,
            IGenderManager genderManager,
            INationalityManager nationalityManager,
            IBloodGroupManager bloodGroupManager,
            IDivisionManager divisionManager,
            IDistrictManager districtManager,
            IUpazilaManager upazilaManager,
            IStudentActivateHistManager studentActivateHistManager)
        {
            _studentManager = studentManager;
            _academicClassManager = academicClassManager;
            _academicSessionManager = academicSessionManager;
            _academicSectionManager = academicSectionManager;
            _religionManager = religionManager;
            _genderManager = genderManager;
            _nationalityManager = nationalityManager;
            _bloodGroupManager = bloodGroupManager;
            _divisionManager = divisionManager;
            _districtManager = districtManager;
            _upazilaManager = upazilaManager;
            _studentActivateHistManager = studentActivateHistManager;
        }

        #region Template

        // Required columns come first so a hand-filled sheet reads naturally.
        private static readonly string[] TemplateColumns =
        {
            "Name", "NameBangla", "ClassRoll", "Class", "Section", "Session",
            "FatherName", "MotherName", "AdmissionDate", "DOB",
            "PhoneNo", "GuardianPhone", "Email",
            "Religion", "Gender", "Nationality", "BloodGroup", "BirthCertificateNo",
            "PresentDivision", "PresentDistrict", "PresentUpazila", "PresentArea", "PresentPO",
            "PermanentDivision", "PermanentDistrict", "PermanentUpazila", "PermanentArea", "PermanentPO",
            "PreviousSchool", "AddressInfo", "IsResidential", "SMSService"
        };

        private static readonly string[] TemplateSampleRow =
        {
            "Md. Rahim Uddin", "মোঃ রহিম উদ্দিন", "1", "Six", "A", "2025-2026",
            "Abdul Karim", "Amena Begum", "2026-01-05", "2012-03-14",
            "01712345678", "01812345678", "rahim@example.com",
            "Islam", "Male", "Bangladeshi", "B+", "19998812345678901",
            "Dhaka", "Dhaka", "Savar", "Bank Colony", "Savar",
            "Dhaka", "Dhaka", "Savar", "Bank Colony", "Savar",
            "Green Model School", "", "No", "Yes"
        };

        // Required columns must carry a value on every row.
        private static readonly string[] RequiredColumns =
        {
            "Name", "ClassRoll", "Class", "Session", "AdmissionDate", "DOB",
            "PhoneNo", "Religion", "Gender", "Nationality",
            "PresentDistrict", "PresentUpazila", "PermanentDistrict", "PermanentUpazila"
        };

        public IReadOnlyList<string> GetTemplateColumns() => TemplateColumns;

        public IReadOnlyList<string> GetTemplateSampleRow() => TemplateSampleRow;

        #endregion Template

        #region Parse & validate

        public async Task<StudentImportResult> ParseAndValidateAsync(Stream fileStream, string fileName)
        {
            var result = new StudentImportResult();

            string extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
            List<string[]> grid;

            try
            {
                grid = extension switch
                {
                    ".csv" => ReadCsv(fileStream),
                    ".xlsx" => ReadXlsx(fileStream),
                    _ => null
                };
            }
            catch (Exception ex)
            {
                result.FileIssues.Add(new StudentImportIssue
                {
                    Message = $"The file could not be read: {ex.Message}"
                });
                return result;
            }

            if (grid == null)
            {
                result.FileIssues.Add(new StudentImportIssue
                {
                    Message = "Only .csv and .xlsx files are supported."
                });
                return result;
            }

            if (grid.Count == 0)
            {
                result.FileIssues.Add(new StudentImportIssue { Message = "The file is empty." });
                return result;
            }

            // Row 1 is the header; map each known column to its position.
            var header = grid[0];
            var columnIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < header.Length; i++)
            {
                string key = NormaliseHeader(header[i]);
                if (string.IsNullOrEmpty(key)) continue;

                var known = TemplateColumns.FirstOrDefault(c => NormaliseHeader(c) == key);
                if (known != null)
                {
                    if (!columnIndex.ContainsKey(known)) columnIndex[known] = i;
                }
                else
                {
                    result.UnknownColumns.Add(header[i].Trim());
                }
            }

            var missingRequired = RequiredColumns.Where(c => !columnIndex.ContainsKey(c)).ToList();
            if (missingRequired.Count > 0)
            {
                result.FileIssues.Add(new StudentImportIssue
                {
                    Message = "The file is missing required column(s): " + string.Join(", ", missingRequired)
                        + ". Download the template to see the expected header."
                });
                return result;
            }

            if (grid.Count == 1)
            {
                result.FileIssues.Add(new StudentImportIssue { Message = "The file has a header but no data rows." });
                return result;
            }

            // Load every lookup table once - never query per row.
            var lookups = await LoadLookupsAsync();

            // Track duplicates inside the batch as well as against the database.
            var rollsInBatch = new Dictionary<int, int>();
            var uniqueIdsInBatch = new Dictionary<string, int>();

            for (int r = 1; r < grid.Count; r++)
            {
                var cells = grid[r];
                int excelRowNumber = r + 1;

                if (cells.All(string.IsNullOrWhiteSpace)) continue;   // skip blank lines

                var row = await BuildRowAsync(cells, columnIndex, excelRowNumber, lookups, rollsInBatch, uniqueIdsInBatch);
                result.Rows.Add(row);
            }

            if (result.Rows.Count == 0)
                result.FileIssues.Add(new StudentImportIssue { Message = "No data rows were found in the file." });

            return result;
        }

        private async Task<StudentImportRow> BuildRowAsync(
            string[] cells,
            Dictionary<string, int> columnIndex,
            int rowNumber,
            LookupCache lookups,
            Dictionary<int, int> rollsInBatch,
            Dictionary<string, int> uniqueIdsInBatch)
        {
            var row = new StudentImportRow { RowNumber = rowNumber };

            string Get(string column)
            {
                if (!columnIndex.TryGetValue(column, out int idx)) return string.Empty;
                return idx < cells.Length ? (cells[idx] ?? string.Empty).Trim() : string.Empty;
            }

            void Error(string column, string value, string message, string suggestion = "")
            {
                row.Issues.Add(new StudentImportIssue
                {
                    RowNumber = rowNumber,
                    Column = column,
                    Value = value,
                    Message = message,
                    Suggestion = suggestion,
                    Level = ImportIssueLevel.Error
                });
            }

            void Warn(string column, string value, string message)
            {
                row.Issues.Add(new StudentImportIssue
                {
                    RowNumber = rowNumber,
                    Column = column,
                    Value = value,
                    Message = message,
                    Level = ImportIssueLevel.Warning
                });
            }

            // ---- required text ----
            row.Name = Get("Name");
            if (string.IsNullOrWhiteSpace(row.Name))
                Error("Name", "", "Student name is required.");
            else if (row.Name.Length > 30)
                Error("Name", row.Name, "Student name must be 30 characters or fewer.");

            // ---- session, class, section ----
            string sessionName = Get("Session");
            row.SessionName = sessionName;
            var session = ResolveByName(lookups.Sessions, sessionName);
            if (session == null)
                Error("Session", sessionName, "Academic session not found.", Suggest(lookups.Sessions.Keys, sessionName));

            string className = Get("Class");
            row.ClassName = className;
            var academicClass = ResolveByName(lookups.Classes, className);
            if (academicClass == null)
                Error("Class", className, "Academic class not found.", Suggest(lookups.Classes.Keys, className));

            int? sectionId = null;
            string sectionName = Get("Section");
            row.SectionName = sectionName;
            if (!string.IsNullOrWhiteSpace(sectionName))
            {
                // A section belongs to a class + session, so only match within those.
                var section = lookups.Sections.FirstOrDefault(s =>
                    string.Equals(s.Name?.Trim(), sectionName, StringComparison.OrdinalIgnoreCase)
                    && (academicClass == null || s.AcademicClassId == academicClass.Id)
                    && (session == null || s.AcademicSessionId == session.Id));

                if (section == null)
                    Error("Section", sectionName, "Section not found for the given class and session.");
                else
                    sectionId = section.Id;
            }

            // ---- roll ----
            string rollText = Get("ClassRoll");
            int providedRoll = 0;
            if (!int.TryParse(rollText, NumberStyles.Integer, CultureInfo.InvariantCulture, out providedRoll)
                || providedRoll < 1 || providedRoll > 999)
            {
                Error("ClassRoll", rollText, "Class roll must be a whole number between 1 and 999.");
            }
            row.ProvidedRoll = providedRoll;

            // ---- dates ----
            DateTime admissionDate = ParseDate(Get("AdmissionDate"), out bool admissionOk);
            if (!admissionOk)
                Error("AdmissionDate", Get("AdmissionDate"), "Use a valid date, preferably yyyy-MM-dd (e.g. 2026-01-05).");

            DateTime dob = ParseDate(Get("DOB"), out bool dobOk);
            if (!dobOk)
                Error("DOB", Get("DOB"), "Use a valid date, preferably yyyy-MM-dd (e.g. 2012-03-14).");
            else if (dob > DateTime.Now)
                Error("DOB", Get("DOB"), "Date of birth cannot be in the future.");

            // ---- phones ----
            string phone = Get("PhoneNo");
            if (!IsValidBdMobile(phone))
                Error("PhoneNo", phone, "Phone must be 11 digits starting 013-019 (e.g. 01712345678).");
            row.PhoneNo = phone;

            string guardianPhone = Get("GuardianPhone");
            if (!string.IsNullOrWhiteSpace(guardianPhone) && !IsValidBdMobile(guardianPhone))
                Error("GuardianPhone", guardianPhone, "Guardian phone must be 11 digits starting 013-019.");

            string email = Get("Email");
            if (!string.IsNullOrWhiteSpace(email) && !email.Contains('@'))
                Error("Email", email, "Email address is not valid.");

            string birthCertificate = Get("BirthCertificateNo");
            if (!string.IsNullOrWhiteSpace(birthCertificate) && birthCertificate.Length != 17)
                Error("BirthCertificateNo", birthCertificate, "Birth certificate number must be exactly 17 digits.");

            // ---- simple lookups ----
            var religion = ResolveByName(lookups.Religions, Get("Religion"));
            if (religion == null)
                Error("Religion", Get("Religion"), "Religion not found.", Suggest(lookups.Religions.Keys, Get("Religion")));

            var gender = ResolveByName(lookups.Genders, Get("Gender"));
            if (gender == null)
                Error("Gender", Get("Gender"), "Gender not found.", Suggest(lookups.Genders.Keys, Get("Gender")));

            var nationality = ResolveByName(lookups.Nationalities, Get("Nationality"));
            if (nationality == null)
                Error("Nationality", Get("Nationality"), "Nationality not found.", Suggest(lookups.Nationalities.Keys, Get("Nationality")));

            int? bloodGroupId = null;
            string bloodGroupName = Get("BloodGroup");
            if (!string.IsNullOrWhiteSpace(bloodGroupName))
            {
                var bloodGroup = ResolveByName(lookups.BloodGroups, bloodGroupName);
                if (bloodGroup == null)
                    Error("BloodGroup", bloodGroupName, "Blood group not found.", Suggest(lookups.BloodGroups.Keys, bloodGroupName));
                else
                    bloodGroupId = bloodGroup.Id;
            }

            // ---- addresses ----
            var presentAddress = ResolveAddress("Present", Get("PresentDivision"), Get("PresentDistrict"), Get("PresentUpazila"),
                                                lookups, Error);
            var permanentAddress = ResolveAddress("Permanent", Get("PermanentDivision"), Get("PermanentDistrict"), Get("PermanentUpazila"),
                                                  lookups, Error);

            // Everything below needs the roll/session/class to be sound.
            if (row.Issues.Any(i => i.Level == ImportIssueLevel.Error))
                return row;

            // ---- derived roll and unique id (same rules as the Create screen) ----
            int generatedRoll = BuildRoll(session, academicClass, providedRoll);
            row.GeneratedRoll = generatedRoll;

            if (rollsInBatch.TryGetValue(generatedRoll, out int firstRow))
            {
                Error("ClassRoll", rollText, $"Duplicate roll - row {firstRow} in this file already uses roll {generatedRoll}.");
                return row;
            }

            var existingByRoll = await _studentManager.GetStudentByClassRollAsync(generatedRoll);
            if (existingByRoll != null)
            {
                Error("ClassRoll", rollText, $"Roll {generatedRoll} already belongs to '{existingByRoll.Name}'.");
                return row;
            }

            string uniqueId = BuildUniqueId(dob, session, generatedRoll);
            row.UniqueId = uniqueId;

            if (uniqueIdsInBatch.TryGetValue(uniqueId, out int firstUidRow))
            {
                Error("DOB", Get("DOB"),
                    $"Generated student ID '{uniqueId}' clashes with row {firstUidRow}. Change the roll or check the date of birth.");
                return row;
            }

            var existingByUid = await _studentManager.GetStudentByUniqueIdAsync(uniqueId);
            if (existingByUid != null)
            {
                Error("DOB", Get("DOB"),
                    $"Generated student ID '{uniqueId}' is already used by '{existingByUid.Name}'. Change the roll or check the date of birth.");
                return row;
            }

            rollsInBatch[generatedRoll] = rowNumber;
            uniqueIdsInBatch[uniqueId] = rowNumber;

            if (string.IsNullOrWhiteSpace(Get("FatherName")) && string.IsNullOrWhiteSpace(Get("MotherName")))
                Warn("FatherName", "", "Neither father's nor mother's name was given.");

            row.Student = new Student
            {
                Name = row.Name,
                NameBangla = Get("NameBangla"),
                ClassRoll = generatedRoll,
                UniqueId = uniqueId,
                AcademicClassId = academicClass.Id,
                AcademicSectionId = sectionId,
                AcademicSessionId = session.Id,
                FatherName = Get("FatherName"),
                MotherName = Get("MotherName"),
                AdmissionDate = admissionDate,
                DOB = dob,
                Email = email,
                PhoneNo = phone,
                GuardianPhone = guardianPhone,
                BirthCertificateNo = birthCertificate,
                ReligionId = religion.Id,
                GenderId = gender.Id,
                NationalityId = nationality.Id,
                BloodGroupId = bloodGroupId,
                PresentDivisionId = presentAddress.DivisionId,
                PresentDistrictId = presentAddress.DistrictId ?? 0,
                PresentUpazilaId = presentAddress.UpazilaId ?? 0,
                PresentAddressArea = Get("PresentArea"),
                PresentAddressPO = Get("PresentPO"),
                PermanentDivisionId = permanentAddress.DivisionId,
                PermanentDistrictId = permanentAddress.DistrictId ?? 0,
                PermanentUpazilaId = permanentAddress.UpazilaId ?? 0,
                PermanentAddressArea = Get("PermanentArea"),
                PermanentAddressPO = Get("PermanentPO"),
                PreviousSchool = Get("PreviousSchool"),
                AddressInfo = Get("AddressInfo") ?? string.Empty,
                IsResidential = ParseBool(Get("IsResidential"), false),
                SMSService = ParseBool(Get("SMSService"), true),
                Status = true
            };

            return row;
        }

        #endregion Parse & validate

        #region Commit

        public async Task<List<Student>> CommitAsync(StudentImportResult result, string userId, string macAddress)
        {
            var saved = new List<Student>();
            if (result == null) return saved;

            var now = DateTime.Now;

            foreach (var row in result.Rows.Where(r => r.IsValid))
            {
                var student = row.Student;
                student.CreatedBy = userId;
                student.CreatedAt = now;
                student.EditedBy = userId;
                student.EditedAt = now;
                student.MACAddress = macAddress ?? string.Empty;

                bool ok = await _studentManager.AddAsync(student);
                if (!ok)
                {
                    result.Rows.First(r => r.RowNumber == row.RowNumber).Issues.Add(new StudentImportIssue
                    {
                        RowNumber = row.RowNumber,
                        Column = "Name",
                        Value = student.Name,
                        Message = "The student could not be saved."
                    });
                    continue;
                }

                await _studentActivateHistManager.AddAsync(new StudentActivateHist
                {
                    StudentId = student.Id,
                    IsActive = true,
                    ActionDateTime = now,
                    LastAction = "Add",
                    CreatedAt = now,
                    CreatedBy = userId,
                    MACAddress = macAddress ?? string.Empty
                });

                saved.Add(student);
            }

            return saved;
        }

        #endregion Commit

        #region Lookups

        private class LookupCache
        {
            public Dictionary<string, AcademicSession> Sessions { get; set; }
            public Dictionary<string, AcademicClass> Classes { get; set; }
            public List<AcademicSection> Sections { get; set; }
            public Dictionary<string, Religion> Religions { get; set; }
            public Dictionary<string, Gender> Genders { get; set; }
            public Dictionary<string, Nationality> Nationalities { get; set; }
            public Dictionary<string, BloodGroup> BloodGroups { get; set; }
            public Dictionary<string, Division> Divisions { get; set; }
            public List<District> Districts { get; set; }
            public List<Upazila> Upazilas { get; set; }
        }

        private async Task<LookupCache> LoadLookupsAsync()
        {
            return new LookupCache
            {
                Sessions = ToDictionary(await _academicSessionManager.GetAllAsync(), s => s.Name),
                Classes = ToDictionary(await _academicClassManager.GetAllAsync(), c => c.Name),
                Sections = (await _academicSectionManager.GetAllAsync()).ToList(),
                Religions = ToDictionary(await _religionManager.GetAllAsync(), r => r.Name),
                Genders = ToDictionary(await _genderManager.GetAllAsync(), g => g.Name),
                Nationalities = ToDictionary(await _nationalityManager.GetAllAsync(), n => n.Name),
                BloodGroups = ToDictionary(await _bloodGroupManager.GetAllAsync(), b => b.Name),
                Divisions = ToDictionary(await _divisionManager.GetAllAsync(), d => d.Name),
                Districts = (await _districtManager.GetAllAsync()).ToList(),
                Upazilas = (await _upazilaManager.GetAllAsync()).ToList()
            };
        }

        private static Dictionary<string, T> ToDictionary<T>(IEnumerable<T> items, Func<T, string> nameSelector)
        {
            var dict = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in items)
            {
                string name = nameSelector(item)?.Trim();
                if (!string.IsNullOrEmpty(name) && !dict.ContainsKey(name)) dict[name] = item;
            }
            return dict;
        }

        private static T ResolveByName<T>(Dictionary<string, T> lookup, string name) where T : class
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            return lookup.TryGetValue(name.Trim(), out var found) ? found : null;
        }

        private struct AddressIds
        {
            public int? DivisionId;
            public int? DistrictId;
            public int? UpazilaId;
        }

        /// <summary>
        /// Resolves division/district/upazila together so a district can be matched
        /// within its division and an upazila within its district - names repeat
        /// across Bangladesh, so matching them in isolation picks the wrong row.
        /// </summary>
        private AddressIds ResolveAddress(string prefix, string divisionName, string districtName, string upazilaName,
                                          LookupCache lookups, Action<string, string, string, string> error)
        {
            var ids = new AddressIds();

            Division division = null;
            if (!string.IsNullOrWhiteSpace(divisionName))
            {
                division = ResolveByName(lookups.Divisions, divisionName);
                if (division == null)
                    error($"{prefix}Division", divisionName, "Division not found.", Suggest(lookups.Divisions.Keys, divisionName));
                else
                    ids.DivisionId = division.Id;
            }

            var districtCandidates = lookups.Districts
                .Where(d => string.Equals(d.Name?.Trim(), districtName?.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (division != null)
                districtCandidates = districtCandidates.Where(d => d.DivisionId == division.Id).ToList();

            District district = districtCandidates.FirstOrDefault();
            if (district == null)
            {
                error($"{prefix}District", districtName,
                    division == null ? "District not found." : $"District not found in division '{divisionName}'.",
                    Suggest(lookups.Districts.Select(d => d.Name), districtName));
            }
            else
            {
                ids.DistrictId = district.Id;
                // Fill the division in when the sheet left it blank.
                if (!ids.DivisionId.HasValue) ids.DivisionId = district.DivisionId;
            }

            var upazilaCandidates = lookups.Upazilas
                .Where(u => string.Equals(u.Name?.Trim(), upazilaName?.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (district != null)
                upazilaCandidates = upazilaCandidates.Where(u => u.DistrictId == district.Id).ToList();

            var upazila = upazilaCandidates.FirstOrDefault();
            if (upazila == null)
            {
                error($"{prefix}Upazila", upazilaName,
                    district == null ? "Upazila not found." : $"Upazila not found in district '{districtName}'.",
                    Suggest(lookups.Upazilas.Select(u => u.Name), upazilaName));
            }
            else
            {
                ids.UpazilaId = upazila.Id;
            }

            return ids;
        }

        /// <summary>Offers the closest known value when a name does not match, e.g. "Islaam" -> "Islam".</summary>
        private static string Suggest(IEnumerable<string> candidates, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            string best = null;
            int bestDistance = int.MaxValue;

            foreach (var candidate in candidates)
            {
                if (string.IsNullOrWhiteSpace(candidate)) continue;
                int distance = Levenshtein(value.Trim().ToLowerInvariant(), candidate.Trim().ToLowerInvariant());
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate.Trim();
                }
            }

            // Only suggest when it is genuinely close, otherwise the hint misleads.
            int tolerance = Math.Max(2, value.Trim().Length / 3);
            return bestDistance <= tolerance ? best : string.Empty;
        }

        private static int Levenshtein(string a, string b)
        {
            if (string.IsNullOrEmpty(a)) return b?.Length ?? 0;
            if (string.IsNullOrEmpty(b)) return a.Length;

            var previous = new int[b.Length + 1];
            var current = new int[b.Length + 1];

            for (int j = 0; j <= b.Length; j++) previous[j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                current[0] = i;
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    current[j] = Math.Min(Math.Min(current[j - 1] + 1, previous[j] + 1), previous[j - 1] + cost);
                }
                Array.Copy(current, previous, current.Length);
            }

            return previous[b.Length];
        }

        #endregion Lookups

        #region Derived values

        /// <summary>
        /// Mirrors StudentsController.CreateRoll: last 2 digits of the session name,
        /// the 2-digit class serial, then the 3-digit roll typed by the user.
        /// </summary>
        private static int BuildRoll(AcademicSession session, AcademicClass academicClass, int providedRoll)
        {
            string year = session.Name.Substring(session.Name.Length - 2);
            string classSerial = academicClass.ClassSerial.ToString("d2");
            string roll = providedRoll.ToString("d3");
            return Convert.ToInt32(year + classSerial + roll);
        }

        /// <summary>
        /// Mirrors StudentsController.GenerateUniquId: DOB as yyMMdd, the last
        /// character of the session name, then the last 2 digits of the full roll.
        /// </summary>
        private static string BuildUniqueId(DateTime dob, AcademicSession session, int generatedRoll)
        {
            string rollText = generatedRoll.ToString();
            string uniqueId = dob.ToString("yyMMdd")
                 + session.Name.Substring(session.Name.Length - 1, 1)
                 + rollText.Substring(rollText.Length - 2, 2);

            // See StudentsController.GenerateUniquId - a birth year ending 00-09 puts a leading
            // zero on this, which must be stripped the same way there so the two never disagree.
            return AttendancePinMatcher.Normalize(uniqueId) ?? uniqueId;
        }

        #endregion Derived values

        #region Value parsing

        private static string NormaliseHeader(string header)
        {
            if (string.IsNullOrWhiteSpace(header)) return string.Empty;
            var sb = new StringBuilder();
            foreach (char c in header)
            {
                if (char.IsLetterOrDigit(c)) sb.Append(char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }

        private static readonly string[] DateFormats =
        {
            "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy",
            "MM/dd/yyyy", "yyyy/MM/dd", "dd.MM.yyyy"
        };

        private static DateTime ParseDate(string value, out bool ok)
        {
            ok = false;
            if (string.IsNullOrWhiteSpace(value)) return default;

            value = value.Trim();

            // Excel hands numeric dates over as a serial number.
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double serial)
                && serial > 20000 && serial < 60000)
            {
                ok = true;
                return DateTime.FromOADate(serial);
            }

            if (DateTime.TryParseExact(value, DateFormats, CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out var exact))
            {
                ok = true;
                return exact;
            }

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var loose))
            {
                ok = true;
                return loose;
            }

            return default;
        }

        private static bool ParseBool(string value, bool fallback)
        {
            if (string.IsNullOrWhiteSpace(value)) return fallback;
            switch (value.Trim().ToLowerInvariant())
            {
                case "yes": case "y": case "true": case "1": return true;
                case "no": case "n": case "false": case "0": return false;
                default: return fallback;
            }
        }

        private static bool IsValidBdMobile(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            phone = phone.Trim();
            if (phone.Length != 11) return false;
            if (!phone.All(char.IsDigit)) return false;
            if (!phone.StartsWith("01")) return false;
            return phone[2] >= '3' && phone[2] <= '9';
        }

        #endregion Value parsing

        #region File readers

        private static List<string[]> ReadXlsx(Stream stream)
        {
            var grid = new List<string[]>();

            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheets.FirstOrDefault();
            if (sheet == null) return grid;

            var range = sheet.RangeUsed();
            if (range == null) return grid;

            int lastColumn = range.LastColumn().ColumnNumber();

            foreach (var xlRow in range.Rows())
            {
                var cells = new string[lastColumn];
                for (int c = 1; c <= lastColumn; c++)
                {
                    var cell = xlRow.Cell(c);
                    // Dates must not go through the culture-dependent default ToString.
                    if (cell.DataType == XLDataType.DateTime && cell.TryGetValue(out DateTime dt))
                        cells[c - 1] = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                    else
                        cells[c - 1] = cell.GetFormattedString()?.Trim() ?? string.Empty;
                }
                grid.Add(cells);
            }

            return grid;
        }

        /// <summary>
        /// Minimal RFC-4180 reader: handles quoted fields, escaped quotes and
        /// newlines inside quotes. Reads as UTF-8 so Bangla names survive.
        /// </summary>
        private static List<string[]> ReadCsv(Stream stream)
        {
            var grid = new List<string[]>();

            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            string content = reader.ReadToEnd();

            var row = new List<string>();
            var field = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < content.Length; i++)
            {
                char ch = content[i];

                if (inQuotes)
                {
                    if (ch == '"')
                    {
                        if (i + 1 < content.Length && content[i + 1] == '"') { field.Append('"'); i++; }
                        else inQuotes = false;
                    }
                    else field.Append(ch);
                    continue;
                }

                switch (ch)
                {
                    case '"':
                        inQuotes = true;
                        break;
                    case ',':
                        row.Add(field.ToString());
                        field.Clear();
                        break;
                    case '\r':
                        break;                       // handled by \n
                    case '\n':
                        row.Add(field.ToString());
                        field.Clear();
                        grid.Add(row.ToArray());
                        row = new List<string>();
                        break;
                    default:
                        field.Append(ch);
                        break;
                }
            }

            // Trailing field / row when the file does not end with a newline.
            if (field.Length > 0 || row.Count > 0)
            {
                row.Add(field.ToString());
                grid.Add(row.ToArray());
            }

            return grid;
        }

        #endregion File readers
    }
}
