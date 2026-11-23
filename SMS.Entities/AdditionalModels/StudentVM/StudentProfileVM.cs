using System;
using System.Collections.Generic;

namespace SMS.Entities.AdditionalModels.StudentVM;

public class StudentProfileVM
{
    public Student Student { get; set; }
    public ProfileAttendance Attendances { get; set; } = new ProfileAttendance();
    public ProfileResult Results { get; set; } = new ProfileResult();
    public ProfilePayment Payments { get; set; } = new ProfilePayment();
    public ProfileDocument Documents { get; set; } = new ProfileDocument();
}

public class ProfileAttendance
{
    public AcademicSession CurrentSession { get; set; }
    public double OverallAttendance { get; set; } = 0.0;
    public int DaysPresent { get; set; } = 0;
    public int DaysAbsent { get; set; } = 0;
    public int LateArrivals { get; set; } = 0;
    public string TodaysAttendance { get; set; }
    public List<MonthlyAttendance> MonthlyAttendances { get; set; } = new List<MonthlyAttendance>();
}
public class MonthlyAttendance
{
    public string MonthName { get; set; }
    public int TotalDays { get; set; }
    public int DaysPresent { get; set; }
    public int DaysAbsent { get; set; }
    public int LateArrivals { get; set; }
    public int Percentage { get; set; }
    public string Status { get; set; }
    public string StatusColor { get; set; }
}

public class ProfileResult {
    public double CurrentCGPA { get; set; }
    public double LastSemesterCGPA { get; set; }
    public int CurrentClassRank { get; set; }
    public string CurrentExamName { get; set; } = "Default Exam Name";
    public string CurrentGrade { get; set; } = "N/F";
    public double CurrentObtainMarks { get; set; }
    public int CurrentTotalFail { get; set; }
    public List<CurrentExamDetail> CurrentExamDetails { get; set; } = [];

    /// <summary>One row per academic session, oldest first, for the progress history.</summary>
    public List<ResultSessionProgress> Sessions { get; set; } = [];

    /// <summary>Per-subject percentage series aligned to <see cref="Sessions"/>.</summary>
    public List<ResultSubjectTrend> SubjectTrends { get; set; } = [];
}
public class CurrentExamDetail
{
    public string SubjectCode { get; set; } = "N/F";
    public string SubjectName { get; set; } = "N/F";
    public double TotalMark { get; set; }
    public string Grade { get; set; } = "N/F";
    public double GPA { get; set; }
    public double TotalObtainMark { get; set; }
}
public class ResultSessionProgress
{
    public int AcademicSessionId { get; set; }
    public string SessionName { get; set; }
    public string ClassName { get; set; }
    public string ExamName { get; set; }
    public int ExamCount { get; set; }
    public double CGPA { get; set; }
    public string Grade { get; set; }
    public int Rank { get; set; }
    public double ObtainedMarks { get; set; }
    public double TotalMarks { get; set; }
    public int TotalFails { get; set; }
    public List<ResultSubjectMark> Subjects { get; set; } = [];
}
public class ResultSubjectMark
{
    public string SubjectName { get; set; }
    public string SubjectCode { get; set; }
    public double ObtainedMarks { get; set; }
    public double TotalMarks { get; set; }
    public double Percentage => TotalMarks > 0 ? (ObtainedMarks * 100.0) / TotalMarks : 0;
    public string Grade { get; set; }
    public double GPA { get; set; }
}
public class ResultSubjectTrend
{
    public string SubjectName { get; set; }
    public string SubjectCode { get; set; }
    public List<double?> Percentages { get; set; } = [];
    public List<string> Grades { get; set; } = [];
}
public class ProfilePayment
{
    public string CurrentSession { get; set; }
    public double TotalFees { get; set; }
    public double TotalPaid { get; set; }
    public double TotalDue { get; set; }
    public DateTime LastPaymentDate { get; set; }
    public List<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();
    public List<UpcommingPayment> UpcomingPayments { get; set; } = new List<UpcommingPayment>();
}
public class PaymentDetail
{
    public string PaymentDate { get; set; }
    public string Description { get; set; }
    public double Amount { get; set; }
    public string Method { get; set; }
    public string TransactionId { get; set; }
}
public class UpcommingPayment
{
    public DateTime DueDate { get; set; }
    public double Amount { get; set; }
    public string Description { get; set; }
}
public class ProfileDocument 
{
    public List<DocInfo> Documents { get; set; }
}

public class DocInfo
{
    public string DocumentName { get; set; }
    public string DocUrl { get; set; }
    public string DocType { get; set; }
}