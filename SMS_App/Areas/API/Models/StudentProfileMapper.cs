using System.Linq;
using StudentEntity = SMS.Entities.Student;

namespace SMS_App.Areas.API.Models;

/// <summary>
/// Projects a student entity (loaded with its lookups, as
/// <c>IStudentManager.GetByIdAsync</c> does) to the mobile profile DTO. Shared by
/// the student self-service profile and the staff student details endpoint so
/// both screens show the same fields.
/// </summary>
public static class StudentProfileMapper
{
    public static StudentProfileDto ToProfileDto(StudentEntity s, string fallbackSessionName = null) => new()
    {
        Id = s.Id,
        UniqueId = s.UniqueId,
        Name = s.Name,
        NameBangla = s.NameBangla,
        ClassRoll = s.ClassRoll,
        ClassName = s.AcademicClass?.Name,
        SectionName = s.AcademicSection?.Name,
        SessionName = s.AcademicSession?.Name ?? fallbackSessionName,
        FatherName = s.FatherName,
        MotherName = s.MotherName,
        FatherNameBangla = s.FatherNameBangla,
        MotherNameBangla = s.MotherNameBangla,
        GuardianPhone = s.GuardianPhone,
        PhoneNo = s.PhoneNo,
        Email = s.Email,
        Address = s.AddressInfo,
        Gender = s.Gender?.Name,
        Nationality = s.Nationality?.Name,
        Religion = s.Religion?.Name,
        BloodGroup = s.BloodGroup?.Name,
        BirthCertificateNo = s.BirthCertificateNo,
        PresentAddress = Address(
            s.PresentAddressArea,
            s.PresentAddressPO,
            s.PresentUpazila?.Name,
            s.PresentDistrict?.Name,
            s.PresentDivision?.Name),
        PermanentAddress = Address(
            s.PermanentAddressArea,
            s.PermanentAddressPO,
            s.PermanentUpazila?.Name,
            s.PermanentDistrict?.Name,
            s.PermanentDivision?.Name),
        PreviousSchool = s.PreviousSchool,
        DateOfBirth = s.DOB,
        AdmissionDate = s.AdmissionDate,
        IsResidential = s.IsResidential,
        Status = s.Status,
        PhotoUrl = PhotoUrl(s.Photo),
    };

    public static string PhotoUrl(string photo) =>
        string.IsNullOrWhiteSpace(photo) ? null : "/Images/Student/" + photo;

    private static string Address(params string[] parts) => string.Join(
        ", ",
        parts.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()));
}
