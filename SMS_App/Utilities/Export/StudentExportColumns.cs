using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SMS.Entities;

namespace SMS_App.Utilities.Export
{
    /// <summary>
    /// Single source of truth for the student export. Both the export modal (which renders a
    /// checkbox per column, grouped) and the <c>Export</c> controller action (which builds the
    /// CSV/XLSX) read this catalog, so the selectable columns and the produced file can never
    /// drift apart. Order here is the order columns appear in the exported file.
    /// </summary>
    public static class StudentExportColumns
    {
        public sealed class Column
        {
            public string Key { get; init; }
            public string Group { get; init; }
            public string Header { get; init; }
            public Func<Student, string> Value { get; init; }
            /// <summary>Pre-ticked in the modal; mirrors the fields the old CSV export produced.</summary>
            public bool Default { get; init; }
        }

        private static string Date(DateTime d) => d == default ? string.Empty : d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        private static string Phone(string p) => string.IsNullOrEmpty(p) ? string.Empty : p.PadLeft(11, '0');

        public static readonly IReadOnlyList<Column> All = new List<Column>
        {
            // Basic
            new() { Key = "uniqueId",   Group = "Basic", Header = "Unique ID",             Value = s => s.UniqueId,                 Default = true },
            new() { Key = "name",       Group = "Basic", Header = "Student Name",          Value = s => s.Name,                     Default = true },
            new() { Key = "nameBangla", Group = "Basic", Header = "Student Name (Bangla)", Value = s => s.NameBangla },
            new() { Key = "status",     Group = "Basic", Header = "Status",                Value = s => s.Status ? "Active" : "Inactive", Default = true },

            // Academic
            new() { Key = "classRoll",      Group = "Academic", Header = "Class Roll",      Value = s => s.ClassRoll.ToString(),         Default = true },
            new() { Key = "className",      Group = "Academic", Header = "Class",           Value = s => s.AcademicClass?.Name ?? "N/A", Default = true },
            new() { Key = "sectionName",    Group = "Academic", Header = "Section",         Value = s => s.AcademicSection?.Name ?? "N/A", Default = true },
            new() { Key = "sessionName",    Group = "Academic", Header = "Session",         Value = s => s.AcademicSession?.Name },
            new() { Key = "admissionDate",  Group = "Academic", Header = "Admission Date",  Value = s => Date(s.AdmissionDate) },
            new() { Key = "previousSchool", Group = "Academic", Header = "Previous School", Value = s => s.PreviousSchool },
            new() { Key = "isResidential",  Group = "Academic", Header = "Residential",     Value = s => s.IsResidential ? "Residential" : "Non Residential", Default = true },
            new() { Key = "machineUserId",  Group = "Academic", Header = "Machine User ID", Value = s => s.MachineUserId },

            // Contact
            new() { Key = "phoneNo", Group = "Contact", Header = "Student Phone", Value = s => Phone(s.PhoneNo), Default = true },
            new() { Key = "email",   Group = "Contact", Header = "Email",         Value = s => s.Email },

            // Guardian / Parents
            new() { Key = "fatherName",    Group = "Guardian / Parents", Header = "Father's Name",  Value = s => s.FatherName },
            new() { Key = "motherName",    Group = "Guardian / Parents", Header = "Mother's Name",  Value = s => s.MotherName },
            new() { Key = "guardianPhone", Group = "Guardian / Parents", Header = "Guardian Phone", Value = s => Phone(s.GuardianPhone), Default = true },

            // Personal
            new() { Key = "gender",      Group = "Personal", Header = "Gender",               Value = s => s.Gender?.Name,      Default = true },
            new() { Key = "dob",         Group = "Personal", Header = "Date of Birth",        Value = s => Date(s.DOB) },
            new() { Key = "religion",    Group = "Personal", Header = "Religion",             Value = s => s.Religion?.Name },
            new() { Key = "bloodGroup",  Group = "Personal", Header = "Blood Group",          Value = s => s.BloodGroup?.Name },
            new() { Key = "nationality", Group = "Personal", Header = "Nationality",          Value = s => s.Nationality?.Name },
            new() { Key = "birthCertNo", Group = "Personal", Header = "Birth Certificate No", Value = s => s.BirthCertificateNo },

            // Present Address
            new() { Key = "presentArea",     Group = "Present Address", Header = "Present Vill/Area",   Value = s => s.PresentAddressArea },
            new() { Key = "presentPO",       Group = "Present Address", Header = "Present Post Office", Value = s => s.PresentAddressPO },
            new() { Key = "presentUpazila",  Group = "Present Address", Header = "Present Upazila",     Value = s => s.PresentUpazila?.Name },
            new() { Key = "presentDistrict", Group = "Present Address", Header = "Present District",    Value = s => s.PresentDistrict?.Name },
            new() { Key = "presentDivision", Group = "Present Address", Header = "Present Division",    Value = s => s.PresentDivision?.Name },

            // Permanent Address
            new() { Key = "permanentArea",     Group = "Permanent Address", Header = "Permanent Vill/Area",   Value = s => s.PermanentAddressArea },
            new() { Key = "permanentPO",       Group = "Permanent Address", Header = "Permanent Post Office", Value = s => s.PermanentAddressPO },
            new() { Key = "permanentUpazila",  Group = "Permanent Address", Header = "Permanent Upazila",     Value = s => s.PermanentUpazila?.Name },
            new() { Key = "permanentDistrict", Group = "Permanent Address", Header = "Permanent District",    Value = s => s.PermanentDistrict?.Name },
            new() { Key = "permanentDivision", Group = "Permanent Address", Header = "Permanent Division",    Value = s => s.PermanentDivision?.Name },

            // Other
            new() { Key = "smsService", Group = "Other", Header = "SMS Service", Value = s => s.SMSService ? "On" : "Off" },
        };

        private static readonly Dictionary<string, Column> ByKey =
            All.ToDictionary(c => c.Key, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Resolves the requested column keys to catalog columns, in catalog order, ignoring
        /// unknown keys. Falls back to the default selection when nothing valid was requested.
        /// </summary>
        public static List<Column> Resolve(IEnumerable<string> keys)
        {
            var requested = new HashSet<string>(keys ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            var selected = All.Where(c => requested.Contains(c.Key)).ToList();
            return selected.Count > 0 ? selected : All.Where(c => c.Default).ToList();
        }

        /// <summary>Catalog grouped for the modal, preserving first-seen group and column order.</summary>
        public static IEnumerable<IGrouping<string, Column>> Grouped() => All.GroupBy(c => c.Group);
    }
}
