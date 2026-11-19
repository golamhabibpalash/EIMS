using System;
using System.Collections.Generic;
using System.Linq;
using SMS.Entities;

namespace SMS_App.Utilities.Students;

/// <summary>
/// Filters, searches, orders and pages the staff student list for the mobile API.
/// Mirrors the web Students grid (<c>StudentsController.List</c>): one academic
/// session, optional class/section, a search over name / Bangla name / roll /
/// unique id / phones, ordered by class serial, section, then roll.
/// </summary>
public static class StudentDirectoryQuery
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 50;

    public static int ClampPage(int page) => Math.Max(1, page);

    public static int ClampPageSize(int pageSize) =>
        pageSize <= 0 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

    public static (List<Student> Page, int TotalCount) Apply(
        IEnumerable<Student> students,
        int? sessionId,
        int? classId,
        int? sectionId,
        string search,
        int page,
        int pageSize)
    {
        page = ClampPage(page);
        pageSize = ClampPageSize(pageSize);

        var query = (students ?? Enumerable.Empty<Student>())
            .Where(s => s.AcademicSessionId == sessionId);

        if (classId != null) query = query.Where(s => s.AcademicClassId == classId.Value);
        if (sectionId != null) query = query.Where(s => s.AcademicSectionId == sectionId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var raw = search.Trim();
            var term = raw.ToLowerInvariant();
            query = query.Where(s =>
                (s.Name?.ToLowerInvariant().Contains(term) ?? false)
                || (s.NameBangla?.Contains(raw) ?? false)
                || s.ClassRoll.ToString().Contains(term)
                || (s.UniqueId?.ToLowerInvariant().Contains(term) ?? false)
                || (s.PhoneNo?.Contains(term) ?? false)
                || (s.GuardianPhone?.Contains(term) ?? false));
        }

        var filtered = query
            .OrderBy(s => s.AcademicClass?.ClassSerial ?? int.MaxValue)
            .ThenBy(s => s.AcademicSection?.Name)
            .ThenBy(s => s.ClassRoll)
            .ThenBy(s => s.Id)
            .ToList();

        var items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return (items, filtered.Count);
    }

    /// <summary>Phones are stored without the leading zero on some instances; show 11 digits like the web grid.</summary>
    public static string Phone(string phone) =>
        string.IsNullOrWhiteSpace(phone) ? null : phone.Trim().PadLeft(11, '0');
}
