using System;
using System.Collections.Generic;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using SMS.Entities.AdditionalModels.StudentVM;

namespace SMS_App.ViewModels.Students;

public class StudentDetailsVM
{
    public PersonalDetails PersonalDetails { get; set; }

    public ProfileDocument Documents { get; set; } = new ProfileDocument();
    public Student Student { get; set; }
    public IReadOnlyCollection<StudentPayment> StudentPayments { get; set; }
    public double TotalDue { get; set; }
    public double CurrentDue { get; set; }
    public List<StudentPaymentScheduleVM> StudentPaymentSchedules { get; set; } = new List<StudentPaymentScheduleVM>();
    public List<StudentPaymentSchedulePaidVM> StudentPaymentSchedulePaidVMs { get; set; } = new List<StudentPaymentSchedulePaidVM>();
    public List<AttendanceIndivisualVM> AttendanceDetails { get; set; }
    public List<StudentActivateHistModel> StatusActivity { get; set; }
}

public class AttendanceIndivisualVM
{
    public string MonthName { get; set; }
    public int AttendanceCount { get; set; }
    public int TotalDays { get; set; }
    public int PresentPercentage { get; set; }
}

public class PersonalDetails
{
    // Personal Information
    public string StudentName { get; set; }
    public string Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int Age => CalculateAge(DateOfBirth);
    public string Nationality { get; set; }
    public string Religion { get; set; }
    public string BloodGroup { get; set; }

    // Contact Information
    public string Phone { get; set; }
    public string Email { get; set; }

    // Guardians Information
    public string FatherName { get; set; }
    public string MotherName { get; set; }
    public string GuardianPhone { get; set; }

    // Address Information
    public string PresentAddress { get; set; }
    public string PermanentAddress { get; set; }

    private int CalculateAge(DateTime dob)
    {
        var today = DateTime.Today;
        int age = today.Year - dob.Year;
        if (dob.Date > today.AddYears(-age)) age--;
        return age;
    }
}