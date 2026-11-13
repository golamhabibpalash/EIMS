using System;
using System.Collections.Generic;

namespace SMS_App.Areas.API.Models;

// Lean, student-facing projections for the mobile app. All data is scoped to the
// signed-in student by StudentController; these never expose another student's data.

public class StudentProfileDto
{
    public int Id { get; set; }
    public string UniqueId { get; set; }
    public string Name { get; set; }
    public string NameBangla { get; set; }
    public int ClassRoll { get; set; }
    public string ClassName { get; set; }
    public string SectionName { get; set; }
    public string SessionName { get; set; }
    public string FatherName { get; set; }
    public string MotherName { get; set; }
    public string GuardianPhone { get; set; }
    public string PhoneNo { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime AdmissionDate { get; set; }
    public bool IsResidential { get; set; }
    public bool Status { get; set; }
    /// <summary>Relative path (e.g. "/Images/Student/x.jpg"); the client prefixes the instance base URL.</summary>
    public string PhotoUrl { get; set; }
}

public class StudentDashboardDto
{
    public string Name { get; set; }
    public string ClassName { get; set; }
    public string SectionName { get; set; }
    public string SessionName { get; set; }
    public int ClassRoll { get; set; }
    public string PhotoUrl { get; set; }

    public double AttendancePercent { get; set; }
    public int DaysPresent { get; set; }
    public int DaysAbsent { get; set; }
    public string TodaysAttendance { get; set; }

    public double TotalDue { get; set; }

    public double CurrentCgpa { get; set; }
    public int CurrentClassRank { get; set; }
    public string CurrentExamName { get; set; }

    public int UnreadNotifications { get; set; }
}

public class AttendanceSummaryDto
{
    public double OverallPercent { get; set; }
    public int DaysPresent { get; set; }
    public int DaysAbsent { get; set; }
    public int LateArrivals { get; set; }
    public string TodaysAttendance { get; set; }
    public List<MonthlyAttendanceDto> Months { get; set; } = new();
}

public class MonthlyAttendanceDto
{
    public string MonthName { get; set; }
    public int TotalDays { get; set; }
    public int DaysPresent { get; set; }
    public int DaysAbsent { get; set; }
    public int LateArrivals { get; set; }
    public int Percentage { get; set; }
    public string Status { get; set; }
}

public class PaymentSummaryDto
{
    public string CurrentSession { get; set; }
    public double TotalFees { get; set; }
    public double TotalPaid { get; set; }
    public double TotalDue { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public List<PaymentItemDto> History { get; set; } = new();
    public List<UpcomingPaymentDto> Upcoming { get; set; } = new();
}

public class PaymentItemDto
{
    public string PaymentDate { get; set; }
    public string Description { get; set; }
    public double Amount { get; set; }
    public string Method { get; set; }
    public string TransactionId { get; set; }
}

public class UpcomingPaymentDto
{
    public DateTime DueDate { get; set; }
    public double Amount { get; set; }
    public string Description { get; set; }
}

public class ResultSummaryDto
{
    public double CurrentCgpa { get; set; }
    public double LastSemesterCgpa { get; set; }
    public int CurrentClassRank { get; set; }
    public string CurrentExamName { get; set; }
    public string CurrentGrade { get; set; }
    public double CurrentObtainMarks { get; set; }
    public int CurrentTotalFail { get; set; }
    public List<ResultSubjectDto> Subjects { get; set; } = new();
}

public class ResultSubjectDto
{
    public string SubjectCode { get; set; }
    public string SubjectName { get; set; }
    public double TotalMark { get; set; }
    public string Grade { get; set; }
    public double Gpa { get; set; }
    public double TotalObtainMark { get; set; }
}

public class AcademicsDto
{
    public string ClassName { get; set; }
    public string SectionName { get; set; }
    public string SessionName { get; set; }
    public int ClassRoll { get; set; }
    public List<SubjectDto> Subjects { get; set; } = new();
}

public class SubjectDto
{
    public string Name { get; set; }
    public string Code { get; set; }
    public double TotalMarks { get; set; }
}

public class NoticeDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Category { get; set; }
    public string Url { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
